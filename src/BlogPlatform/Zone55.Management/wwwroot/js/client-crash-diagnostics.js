(function () {
    'use strict';

    const maxMessageLength = 16000;
    const maxBufferedEvents = 300;
    const flushIntervalMs = 3000;
    const maxBatchSize = 20;
    const transportTimeoutMs = 2000;
    const maxBackoffMs = 30000;
    const persistentStorageKey = 'zone55.management.crash-trace.v2';
    const maxPersistentEvents = 250;
    const performanceStart = performance.now();
    const state = {
        initialized: false,
        apiBaseUrl: null,
        source: 'UNKNOWN',
        sessionId: crypto?.randomUUID?.().replaceAll('-', '') ?? `${Date.now()}-${Math.random()}`,
        sequence: 0,
        context: {},
        buffered: [],
        sending: false,
        flushTimer: null,
        retryTimer: null,
        consecutiveFailures: 0,
        nextFlushAllowedAt: 0,
        originalConsoleError: console.error.bind(console),
        originalConsoleWarn: console.warn.bind(console),
        lastHeartbeatAt: performance.now(),
        heartbeatTimer: null,
        performanceObservers: [],
        persistent: [],
        heartbeatSequence: 0,
        lastCriticalEvent: null
    };

    const safeString = value => {
        try {
            if (value instanceof Error) return `${value.name}: ${value.message}\n${value.stack ?? ''}`;
            if (typeof value === 'string') return value;
            return JSON.stringify(value, (_key, nested) => {
                if (nested instanceof Error) return { name: nested.name, message: nested.message, stack: nested.stack };
                if (nested instanceof EventTarget) return `[${nested.constructor?.name ?? 'EventTarget'}]`;
                return nested;
            });
        } catch {
            try { return String(value); } catch { return '[unprintable value]'; }
        }
    };

    const memorySnapshot = () => {
        const memory = performance.memory;
        return memory ? {
            jsHeapUsed: memory.usedJSHeapSize,
            jsHeapTotal: memory.totalJSHeapSize,
            jsHeapLimit: memory.jsHeapSizeLimit
        } : {};
    };

    const buildContext = extra => ({
        url: location.href,
        origin: location.origin,
        referrer: document.referrer || '-',
        isIframe: window.parent !== window,
        visibility: document.visibilityState,
        readyState: document.readyState,
        online: navigator.onLine,
        hardwareConcurrency: navigator.hardwareConcurrency,
        deviceMemoryGb: navigator.deviceMemory ?? null,
        elapsedMs: Math.round(performance.now() - performanceStart),
        ...memorySnapshot(),
        ...state.context,
        ...extra
    });


    const loadPersistent = () => {
        try {
            const raw = localStorage.getItem(persistentStorageKey);
            if (!raw) return [];
            const parsed = JSON.parse(raw);
            return Array.isArray(parsed) ? parsed : [];
        } catch {
            return [];
        }
    };

    const savePersistent = () => {
        try {
            localStorage.setItem(persistentStorageKey, JSON.stringify(state.persistent.slice(-maxPersistentEvents)));
        } catch {
            // Storage diagnostics must never affect the application.
        }
    };

    const persistCritical = (eventName, message, extra) => {
        const entry = {
            sessionId: state.sessionId,
            source: state.source,
            sequence: state.sequence + 1,
            event: eventName,
            message: String(message || '-'),
            utc: new Date().toISOString(),
            performanceNowMs: Math.round(performance.now()),
            context: buildContext(extra)
        };
        state.lastCriticalEvent = entry;
        state.persistent.push(entry);
        if (state.persistent.length > maxPersistentEvents) state.persistent.shift();
        savePersistent();
        return entry;
    };

    const buildMessage = (message, extra) => {
        const full = `${message}\nContext=${safeString(buildContext(extra))}`;
        return full.length <= maxMessageLength ? full : `${full.slice(0, maxMessageLength)} [truncated]`;
    };

    const enqueue = (eventName, message, extra) => {
        const entry = {
            Source: state.source,
            SessionId: state.sessionId,
            Event: eventName,
            Sequence: ++state.sequence,
            Message: buildMessage(message, extra)
        };
        state.buffered.push(entry);
        if (state.buffered.length > maxBufferedEvents) state.buffered.shift();
        scheduleFlush();
    };

    const recordCritical = (eventName, message, extra) => {
        persistCritical(eventName, message, extra);
        enqueue(eventName, message, { ...(extra || {}), persistentCritical: true });
    };

    const endpoint = () => state.apiBaseUrl ? new URL('api/preview-diagnostics/batch', state.apiBaseUrl).toString() : null;

    const scheduleFlush = (delayMs = flushIntervalMs) => {
        if (!state.initialized || state.flushTimer || state.retryTimer || state.buffered.length === 0) return;
        const remainingBackoff = Math.max(0, state.nextFlushAllowedAt - Date.now());
        const effectiveDelay = Math.max(delayMs, remainingBackoff);
        state.flushTimer = window.setTimeout(() => {
            state.flushTimer = null;
            flush();
        }, effectiveDelay);
    };

    const scheduleRetry = () => {
        if (state.retryTimer || state.buffered.length === 0) return;
        const delayMs = Math.min(maxBackoffMs, 1000 * (2 ** Math.min(state.consecutiveFailures, 5)));
        state.nextFlushAllowedAt = Date.now() + delayMs;
        state.retryTimer = window.setTimeout(() => {
            state.retryTimer = null;
            scheduleFlush(0);
        }, delayMs);
    };

    const flush = () => {
        if (!state.initialized || state.sending || state.buffered.length === 0) return;
        if (Date.now() < state.nextFlushAllowedAt) {
            scheduleRetry();
            return;
        }

        const url = endpoint();
        if (!url) return;

        const entries = state.buffered.splice(0, maxBatchSize);
        state.sending = true;

        let completed = false;
        const request = new XMLHttpRequest();
        const finish = succeeded => {
            if (completed) return;
            completed = true;
            state.sending = false;

            if (succeeded) {
                state.consecutiveFailures = 0;
                state.nextFlushAllowedAt = 0;
            } else {
                state.buffered.unshift(...entries);
                if (state.buffered.length > maxBufferedEvents) {
                    state.buffered.splice(0, state.buffered.length - maxBufferedEvents);
                }
                state.consecutiveFailures++;
                scheduleRetry();
                return;
            }

            if (state.buffered.length > 0) scheduleFlush(250);
        };

        try {
            request.open('POST', url, true);
            request.timeout = transportTimeoutMs;
            request.setRequestHeader('Content-Type', 'application/json');
            request.onload = () => finish(request.status >= 200 && request.status < 300);
            request.onerror = () => finish(false);
            request.ontimeout = () => finish(false);
            request.onabort = () => finish(false);
            request.send(JSON.stringify({ entries }));
        } catch {
            finish(false);
        }
    };

    const tryBeaconFlush = () => {
        try {
            const url = endpoint();
            if (!url || state.buffered.length === 0 || !navigator.sendBeacon) return;
            const entries = state.buffered.slice(0, maxBatchSize);
            navigator.sendBeacon(url, new Blob([JSON.stringify({ entries })], { type: 'application/json' }));
        } catch {
            // Best effort only.
        }
    };

    const resourceDetails = entry => ({
        name: entry.name,
        initiatorType: entry.initiatorType,
        durationMs: Math.round(entry.duration),
        startTimeMs: Math.round(entry.startTime),
        transferSize: entry.transferSize,
        encodedBodySize: entry.encodedBodySize,
        decodedBodySize: entry.decodedBodySize,
        responseStartMs: Math.round(entry.responseStart),
        responseEndMs: Math.round(entry.responseEnd),
        protocol: entry.nextHopProtocol || '-',
        renderBlockingStatus: entry.renderBlockingStatus || '-'
    });

    const shouldTrackResource = entry => {
        const name = entry.name || '';
        return name.includes('/_framework/') || name.includes('blazor.webassembly.js') ||
            name.includes('dotnet.') || name.includes('dotnet.native') || name.includes('dotnet.runtime') ||
            name.includes('prism') || name.includes('mermaid') || name.includes('bootstrap') ||
            name.includes('/preview/article') || entry.duration >= 1000;
    };

    const installPerformanceObservers = () => {
        try {
            const longTaskObserver = new PerformanceObserver(list => {
                for (const entry of list.getEntries()) {
                    if (entry.duration < 500) continue;
                    enqueue('MainThreadLongTask', `DurationMs=${Math.round(entry.duration)}; StartMs=${Math.round(entry.startTime)}`, {
                        durationMs: Math.round(entry.duration),
                        startTimeMs: Math.round(entry.startTime),
                        attribution: entry.attribution?.map(item => ({
                            name: item.name,
                            containerType: item.containerType,
                            containerName: item.containerName,
                            containerSrc: item.containerSrc,
                            containerId: item.containerId
                        })) ?? []
                    });
                }
            });
            longTaskObserver.observe({ type: 'longtask', buffered: true });
            state.performanceObservers.push(longTaskObserver);
        } catch (error) {
            enqueue('PerformanceObserverUnavailable', `Long-task observer failed: ${safeString(error)}`);
        }
    };

    const startHeartbeat = () => {
        const intervalMs = 2000;
        if (state.heartbeatTimer) return;
        state.lastHeartbeatAt = performance.now();
        state.heartbeatTimer = window.setInterval(() => {
            const now = performance.now();
            const lag = now - state.lastHeartbeatAt - intervalMs;
            state.lastHeartbeatAt = now;
            state.heartbeatSequence++;

            const heartbeat = {
                sessionId: state.sessionId,
                source: state.source,
                sequence: state.heartbeatSequence,
                event: 'PersistentHeartbeat',
                message: `Heartbeat=${state.heartbeatSequence}; LagMs=${Math.round(lag)}; LastCritical=${state.lastCriticalEvent?.event || '-'}`,
                utc: new Date().toISOString(),
                performanceNowMs: Math.round(now),
                context: buildContext({
                    lagMs: Math.round(lag),
                    lastCriticalEvent: state.lastCriticalEvent?.event || '-',
                    lastCriticalUtc: state.lastCriticalEvent?.utc || '-'
                })
            };

            state.persistent.push(heartbeat);
            if (state.persistent.length > maxPersistentEvents) state.persistent.shift();
            savePersistent();

            if (lag >= 250) {
                enqueue('EventLoopLag', `LagMs=${Math.round(lag)}`, { lagMs: Math.round(lag) });
            }
        }, intervalMs);
    };

    const recordNavigationTiming = () => {
        const navigation = performance.getEntriesByType('navigation')[0];
        if (!navigation) return;
        enqueue('NavigationTiming', `Type=${navigation.type}; DomInteractiveMs=${Math.round(navigation.domInteractive)}; LoadEventEndMs=${Math.round(navigation.loadEventEnd)}`, {
            type: navigation.type,
            redirectCount: navigation.redirectCount,
            dnsMs: Math.round(navigation.domainLookupEnd - navigation.domainLookupStart),
            connectMs: Math.round(navigation.connectEnd - navigation.connectStart),
            tlsMs: navigation.secureConnectionStart > 0 ? Math.round(navigation.connectEnd - navigation.secureConnectionStart) : 0,
            requestToFirstByteMs: Math.round(navigation.responseStart - navigation.requestStart),
            responseMs: Math.round(navigation.responseEnd - navigation.responseStart),
            domInteractiveMs: Math.round(navigation.domInteractive),
            domContentLoadedMs: Math.round(navigation.domContentLoadedEventEnd),
            loadEventEndMs: Math.round(navigation.loadEventEnd),
            transferSize: navigation.transferSize,
            decodedBodySize: navigation.decodedBodySize
        });
    };

    window.addEventListener('error', event => {
        const target = event.target;
        if (target && target !== window) {
            const tag = target.tagName ?? target.nodeName ?? 'unknown';
            const resource = target.src ?? target.href ?? '-';
            enqueue('ResourceLoadError', `Resource failed to load. Tag=${tag}; Resource=${resource}`, {
                resourceTag: tag,
                resourceUrl: resource,
                outerHtml: target.outerHTML?.slice(0, 1000) ?? '-'
            });
            return;
        }
        enqueue('GlobalError', `${event.message ?? 'Unknown window error'}\n${event.error?.stack ?? ''}`, {
            filename: event.filename ?? '-', line: event.lineno ?? 0, column: event.colno ?? 0,
            errorName: event.error?.name ?? '-'
        });
    }, true);

    window.addEventListener('unhandledrejection', event => enqueue('UnhandledPromiseRejection', safeString(event.reason), {
        reasonType: event.reason?.constructor?.name ?? typeof event.reason
    }));
    window.addEventListener('securitypolicyviolation', event => enqueue('SecurityPolicyViolation', `Blocked=${event.blockedURI}; Directive=${event.violatedDirective}`, {
        disposition: event.disposition, sourceFile: event.sourceFile, line: event.lineNumber, column: event.columnNumber
    }));
    window.addEventListener('offline', () => enqueue('NetworkOffline', 'Browser reported offline state.'));
    window.addEventListener('online', () => enqueue('NetworkOnline', 'Browser reported online state.'));
    window.addEventListener('pagehide', event => { recordCritical('PageHide', `Persisted=${event.persisted}`); tryBeaconFlush(); });
    window.addEventListener('pageshow', event => enqueue('PageShow', `Persisted=${event.persisted}`));
    window.addEventListener('beforeunload', () => { recordCritical('BeforeUnload', 'Page is unloading.'); tryBeaconFlush(); });
    window.addEventListener('load', () => { enqueue('WindowLoad', 'Window load event fired.'); recordNavigationTiming(); }, { once: true });
    window.addEventListener('popstate', () => enqueue('NavigationPopState', `Url=${location.href}`));
    window.addEventListener('hashchange', event => enqueue('NavigationHashChange', `Old=${event.oldURL}; New=${event.newURL}`));
    document.addEventListener('DOMContentLoaded', () => enqueue('DomContentLoaded', 'DOMContentLoaded fired.'), { once: true });
    document.addEventListener('visibilitychange', () => enqueue('VisibilityChanged', `Visibility=${document.visibilityState}`));
    document.addEventListener('pointerdown', event => {
        const button = event.target?.closest?.('button');
        if (!button) return;
        recordCritical('PointerDown.Button', `Text=${button.textContent?.trim().slice(0, 200) ?? '-'}; Class=${button.className || '-'}; Disabled=${!!button.disabled}`, {
            pointerType: event.pointerType || '-',
            buttonText: button.textContent?.trim().slice(0, 200) ?? '-',
            buttonClass: button.className || '-',
            activeElement: document.activeElement?.tagName || '-'
        });
    }, true);

    document.addEventListener('click', event => {
        const anchor = event.target?.closest?.('a[href]');
        if (anchor) enqueue('NavigationLinkClicked', `Href=${anchor.href}; Text=${anchor.textContent?.trim().slice(0, 200) ?? '-'}`);
    }, true);

    const originalPushState = history.pushState.bind(history);
    history.pushState = function (...args) { const result = originalPushState(...args); enqueue('NavigationPushState', `Url=${location.href}`); return result; };
    const originalReplaceState = history.replaceState.bind(history);
    history.replaceState = function (...args) { const result = originalReplaceState(...args); enqueue('NavigationReplaceState', `Url=${location.href}`); return result; };

    console.error = function (...args) { state.originalConsoleError(...args); enqueue('ConsoleError', args.map(safeString).join('\n')); };
    console.warn = function (...args) {
        state.originalConsoleWarn(...args);
        const text = args.map(safeString).join('\n');
        if (/blazor|webassembly|wasm|live_preview|failed|error/i.test(text)) enqueue('ConsoleWarning', text);
    };

    const observeBlazorErrorUi = () => {
        const errorUi = document.getElementById('blazor-error-ui');
        if (!errorUi) { enqueue('BlazorErrorUiMissing', 'The #blazor-error-ui element was not found.'); return; }
        let visiblePreviously = false;
        const inspect = () => {
            const style = getComputedStyle(errorUi);
            const visible = style.display !== 'none' && style.visibility !== 'hidden' && errorUi.getClientRects().length > 0;
            if (visible && !visiblePreviously) enqueue('BlazorErrorUiVisible', `Text=${errorUi.textContent?.trim() ?? '-'}`, {
                display: style.display, visibility: style.visibility, className: errorUi.className,
                inlineStyle: errorUi.getAttribute('style') ?? '-', appHtml: document.getElementById('app')?.innerHTML?.slice(0, 4000) ?? '-'
            });
            visiblePreviously = visible;
        };
        new MutationObserver(inspect).observe(errorUi, { attributes: true, childList: true, subtree: true, attributeFilter: ['style', 'class', 'hidden'] });
        inspect();
    };

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', observeBlazorErrorUi, { once: true });
    else observeBlazorErrorUi();

    installPerformanceObservers();

    window.zone55CrashDiagnostics = {
        initialize: async (apiBaseUrl, source, initialContext) => {
            const recovered = loadPersistent();
            state.persistent = recovered;
            state.apiBaseUrl = apiBaseUrl;
            state.source = source || state.source;
            state.context = { ...state.context, ...(initialContext || {}) };
            state.initialized = true;
            enqueue('DiagnosticsInitialized', `Source=${state.source}; Session=${state.sessionId}`);

            const previousSessionEvents = recovered.filter(item => item?.sessionId && item.sessionId !== state.sessionId).slice(-120);
            if (previousSessionEvents.length > 0) {
                const criticalTail = previousSessionEvents
                    .filter(item => item.event !== 'PersistentHeartbeat')
                    .slice(-40);
                const heartbeatTail = previousSessionEvents
                    .filter(item => item.event === 'PersistentHeartbeat')
                    .slice(-8);
                const compactTrace = [...criticalTail, ...heartbeatTail]
                    .sort((left, right) => String(left.utc).localeCompare(String(right.utc)))
                    .map(item => ({
                        sessionId: item.sessionId,
                        source: item.source,
                        event: item.event,
                        message: item.message,
                        utc: item.utc,
                        performanceNowMs: item.performanceNowMs,
                        url: item.context?.url || '-',
                        visibility: item.context?.visibility || '-',
                        readyState: item.context?.readyState || '-',
                        workspaceMode: item.context?.workspaceMode || '-',
                        phase: item.context?.phase || '-',
                        lagMs: item.context?.lagMs ?? null,
                        jsHeapUsed: item.context?.jsHeapUsed ?? null
                    }));

                enqueue('RecoveredPersistentTrace', `RecoveredEvents=${previousSessionEvents.length}; CompactEvents=${compactTrace.length}; LastEvent=${previousSessionEvents[previousSessionEvents.length - 1]?.event || '-'}`, {
                    recoveredEvents: compactTrace
                });
            }

            persistCritical('Diagnostics.PersistentSessionStarted', `Session=${state.sessionId}; Recovered=${previousSessionEvents.length}`, { source: state.source });
            startHeartbeat();
            scheduleFlush(100);
            return state.sessionId;
        },
        setContext: context => { state.context = { ...state.context, ...(context || {}) }; },
        record: (eventName, message, extra) => enqueue(eventName, message, extra),
        recordCritical: (eventName, message, extra) => recordCritical(eventName, message, extra),
        mark: (eventName, extra) => enqueue(eventName, `PerformanceNowMs=${Math.round(performance.now())}`, extra),
        getSessionId: () => state.sessionId,
        getPersistentTrace: () => [...state.persistent],
        clearPersistentTrace: () => { state.persistent = []; savePersistent(); }
    };

    state.persistent = loadPersistent();
    persistCritical('DiagnosticsScriptLoaded.Persistent', 'Client crash diagnostics script loaded before Blazor startup.');
    enqueue('DiagnosticsScriptLoaded', 'Client crash diagnostics script loaded before Blazor startup.');
})();
