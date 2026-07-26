let sequence = 0;
const previewStates = new WeakMap();
const hostStates = new WeakMap();

function clearRetryTimers(state) {
    for (const timer of state.retryTimers) {
        window.clearTimeout(timer);
    }

    state.retryTimers.length = 0;
}

function postLatest(state) {
    if (!state || !state.frame?.contentWindow || !state.article) {
        return;
    }

    try {
        state.frame.contentWindow.postMessage({
            type: 'BLOG_ARTICLE_PREVIEW',
            sequence: ++sequence,
            article: state.article
        }, state.portalOrigin);
    } catch {
        // Preview is best effort. Transport failures must never affect the editor.
    }
}

function scheduleLoadRetries(state) {
    clearRetryTimers(state);

    for (const delay of [250, 750, 1500]) {
        state.retryTimers.push(window.setTimeout(() => postLatest(state), delay));
    }
}

export function connectArticlePreview(frame, portalOrigin) {
    if (!frame || !portalOrigin) {
        return;
    }

    const existing = previewStates.get(frame);
    if (existing) {
        existing.portalOrigin = portalOrigin;
        return;
    }

    const state = {
        frame,
        portalOrigin,
        article: null,
        retryTimers: [],
        loadHandler: null
    };

    state.loadHandler = () => {
        postLatest(state);
        scheduleLoadRetries(state);
    };

    frame.addEventListener('load', state.loadHandler);
    previewStates.set(frame, state);
}

export function sendArticlePreview(frame, article) {
    const state = previewStates.get(frame);
    if (!state) {
        return;
    }

    state.article = article;
    postLatest(state);
}

function hideHost(host) {
    host.style.left = '-10000px';
    host.style.top = '0';
    host.style.width = '1px';
    host.style.height = '1px';
    host.style.visibility = 'hidden';
    host.style.pointerEvents = 'none';
}

function syncHost(state) {
    state.animationFrame = 0;

    if (!state.visible) {
        hideHost(state.host);
        return;
    }

    const target = document.getElementById(state.targetElementId);
    if (!target) {
        hideHost(state.host);
        return;
    }

    const rect = target.getBoundingClientRect();
    if (rect.width <= 1 || rect.height <= 1) {
        hideHost(state.host);
        return;
    }

    state.host.style.left = `${Math.round(rect.left)}px`;
    state.host.style.top = `${Math.round(rect.top)}px`;
    state.host.style.width = `${Math.round(rect.width)}px`;
    state.host.style.height = `${Math.round(rect.height)}px`;
    state.host.style.visibility = 'visible';
    state.host.style.pointerEvents = 'auto';
}

function requestHostSync(state) {
    if (state.animationFrame) {
        return;
    }

    state.animationFrame = window.requestAnimationFrame(() => syncHost(state));
}

function observeTarget(state) {
    const target = document.getElementById(state.targetElementId);
    if (state.observedTarget === target) {
        return;
    }

    state.resizeObserver.disconnect();
    state.observedTarget = target;

    if (target) {
        state.resizeObserver.observe(target);
    }
}

export function setPersistentPreviewHost(host, targetElementId, visible) {
    if (!host || !targetElementId) {
        return;
    }

    let state = hostStates.get(host);
    if (!state) {
        state = {
            host,
            targetElementId,
            visible: false,
            observedTarget: null,
            animationFrame: 0,
            resizeObserver: null,
            syncHandler: null
        };

        state.syncHandler = () => requestHostSync(state);
        state.resizeObserver = new ResizeObserver(state.syncHandler);

        window.addEventListener('resize', state.syncHandler, { passive: true });
        window.addEventListener('scroll', state.syncHandler, { passive: true, capture: true });
        hostStates.set(host, state);
    }

    state.targetElementId = targetElementId;
    state.visible = Boolean(visible);
    observeTarget(state);
    requestHostSync(state);
}
