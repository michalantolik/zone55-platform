let sequence = 0;
const frameStates = new WeakMap();

function clearStateTimers(state) {
    if (!state) return;

    window.clearTimeout(state.unavailableTimer);
    window.clearTimeout(state.renderTimer);
    state.unavailableTimer = null;
    state.renderTimer = null;
}

function diagnostics(state, eventName, sequenceNumber, message) {
    if (!state?.diagnosticsEnabled || state.disconnected || !state.dotNetObject) return;

    state.dotNetObject
        .invokeMethodAsync('NotifyPreviewDiagnostic', eventName, sequenceNumber || 0, String(message || '-'))
        .catch(error => console.warn('[LIVE_PREVIEW] diagnostic callback failed.', error));
}

function markDisconnected(state, reason) {
    if (!state || state.disconnected) return;

    state.disconnected = true;
    clearStateTimers(state);
    console.warn('[LIVE_PREVIEW] Preview bridge disconnected.', reason);
}

function invokeDotNetSafely(state, methodName, ...args) {
    if (!state || state.disconnected || !state.dotNetObject) return;

    state.dotNetObject.invokeMethodAsync(methodName, ...args).catch(error => {
        const details = `${methodName}: ${error?.stack || error}`;
        console.warn('[LIVE_PREVIEW] .NET callback failed.', details);
        markDisconnected(state, details);
    });
}

function postLatest(frame) {
    const state = frameStates.get(frame);
    if (!frame?.contentWindow || !state || state.disconnected || !state.article || !state.ready) {
        diagnostics(state, 'PostSkipped', state?.lastSentSequence || 0,
            `HasContentWindow=${!!frame?.contentWindow}; HasState=${!!state}; Disconnected=${!!state?.disconnected}; HasArticle=${!!state?.article}; Ready=${!!state?.ready}`);
        return;
    }

    const currentSequence = ++sequence;
    state.lastSentSequence = currentSequence;
    diagnostics(state, 'PostMessageSending', currentSequence,
        `TargetOrigin=${state.portalOrigin}; Slug=${state.article.slug}; TitleLength=${state.article.title?.length || 0}; BodyLength=${state.article.bodyHtml?.length || 0}`);

    try {
        frame.contentWindow.postMessage({
            type: 'BLOG_ARTICLE_PREVIEW',
            sequence: currentSequence,
            sessionId: state.sessionId,
            article: state.article
        }, state.portalOrigin);

        diagnostics(state, 'PostMessageSent', currentSequence, `TargetOrigin=${state.portalOrigin}`);
        scheduleRenderTimeout(state, currentSequence);
    } catch (error) {
        const details = error?.stack || error?.message || String(error);
        diagnostics(state, 'PostMessageFailed', currentSequence, details);
        invokeDotNetSafely(state, 'NotifyPreviewFailed', `Preview message could not be sent. ${details}`);
    }
}

function handleMessage(frame, state, event) {
    if (state.disconnected) return;
    if (event.source !== frame.contentWindow) return;
    if (event.origin !== state.portalOrigin) {
        diagnostics(state, 'MessageRejectedOrigin', 0, `Expected=${state.portalOrigin}; Actual=${event.origin}`);
        return;
    }
    if (!event.data) return;

    diagnostics(state, 'MessageReceived', Number(event.data.sequence || 0), `Type=${event.data.type || 'unknown'}`);

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_READY') {
        window.clearTimeout(state.unavailableTimer);
        state.unavailableTimer = null;
        state.ready = true;
        invokeDotNetSafely(state, 'NotifyPreviewReady');
        postLatest(frame);
        return;
    }

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_ACK') {
        if (event.data.sequence !== state.lastSentSequence) {
            diagnostics(state, 'AckIgnoredSequence', Number(event.data.sequence || 0), `Expected=${state.lastSentSequence}`);
            return;
        }

        window.clearTimeout(state.renderTimer);
        state.renderTimer = null;
        diagnostics(state, 'AckAccepted', event.data.sequence,
            `Title=${event.data.title || ''}; Blocks=${event.data.blockCount || 0}; BlockFailures=${event.data.blockFailureCount || 0}`);
        invokeDotNetSafely(state, 'NotifyPreviewRendered', Number(event.data.blockFailureCount || 0));
        return;
    }

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_ERROR') {
        const errorSequence = Number(event.data.sequence || 0);
        if (errorSequence !== 0 && errorSequence !== state.lastSentSequence) {
            diagnostics(state, 'ErrorIgnoredSequence', errorSequence, `Expected=${state.lastSentSequence}`);
            return;
        }

        window.clearTimeout(state.renderTimer);
        state.renderTimer = null;
        invokeDotNetSafely(state, 'NotifyPreviewFailed', String(event.data.message || 'Unknown preview error'));
    }
}

function scheduleUnavailableCheck(state) {
    window.clearTimeout(state.unavailableTimer);
    diagnostics(state, 'ReadyTimeoutScheduled', 0, 'TimeoutMs=15000');

    state.unavailableTimer = window.setTimeout(() => {
        state.unavailableTimer = null;
        if (!state.disconnected && !state.ready) {
            diagnostics(state, 'ReadyTimeoutElapsed', 0, 'Portal did not signal ready.');
            invokeDotNetSafely(state, 'NotifyPreviewUnavailable');
        }
    }, 15000);
}

function scheduleRenderTimeout(state, expectedSequence) {
    window.clearTimeout(state.renderTimer);
    diagnostics(state, 'RenderTimeoutScheduled', expectedSequence, 'TimeoutMs=15000');

    state.renderTimer = window.setTimeout(() => {
        state.renderTimer = null;
        if (!state.disconnected && state.lastSentSequence === expectedSequence) {
            diagnostics(state, 'RenderTimeoutElapsed', expectedSequence, 'No ACK received.');
            invokeDotNetSafely(state, 'NotifyPreviewFailed',
                `Preview did not acknowledge render sequence ${expectedSequence} within 15 seconds.`);
        }
    }, 15000);
}

export function connectArticlePreview(frame, dotNetObject, portalOrigin, diagnosticsEnabled, sessionId) {
    if (!frame || !dotNetObject || !portalOrigin) return;

    disconnectArticlePreview(frame);

    const state = {
        article: null,
        dotNetObject,
        portalOrigin,
        diagnosticsEnabled,
        sessionId,
        unavailableTimer: null,
        renderTimer: null,
        messageHandler: null,
        loadHandler: null,
        disconnected: false,
        ready: false,
        lastSentSequence: 0
    };

    state.messageHandler = event => handleMessage(frame, state, event);
    state.loadHandler = () => {
        if (state.disconnected) return;

        state.ready = false;
        window.clearTimeout(state.renderTimer);
        state.renderTimer = null;
        diagnostics(state, 'IframeLoaded', 0,
            `Src=${frame.src}; Referrer=${document.referrer || '-'}; Visibility=${document.visibilityState}`);
        scheduleUnavailableCheck(state);
    };

    frameStates.set(frame, state);
    window.addEventListener('message', state.messageHandler);
    frame.addEventListener('load', state.loadHandler);
    diagnostics(state, 'BridgeConnected', 0, `PortalOrigin=${portalOrigin}; FrameSrc=${frame.src}`);
    scheduleUnavailableCheck(state);
}

export function sendArticlePreview(frame, article) {
    const state = frameStates.get(frame);
    if (!state || state.disconnected) return;

    state.article = article;
    diagnostics(state, 'PayloadStored', 0,
        `Ready=${state.ready}; Slug=${article?.slug || '-'}; BodyLength=${article?.bodyHtml?.length || 0}`);
    postLatest(frame);
}

export function disconnectArticlePreview(frame) {
    const state = frameStates.get(frame);
    if (!state) return;

    diagnostics(state, 'BridgeDisconnecting', state.lastSentSequence, '-');
    state.disconnected = true;
    clearStateTimers(state);
    window.removeEventListener('message', state.messageHandler);
    frame.removeEventListener('load', state.loadHandler);
    state.dotNetObject = null;
    state.article = null;
    frameStates.delete(frame);
}
