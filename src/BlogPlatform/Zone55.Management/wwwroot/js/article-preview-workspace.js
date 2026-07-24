let sequence = 0;
const frameStates = new WeakMap();

function invokeDotNetSafely(state, methodName, ...args) {
    if (state.disconnected) {
        return;
    }

    state.dotNetObject.invokeMethodAsync(methodName, ...args).catch(error => {
        console.warn(`[LIVE_PREVIEW] ${methodName} callback failed.`, error);
    });
}

function postLatest(frame) {
    const state = frameStates.get(frame);

    if (!frame?.contentWindow || !state?.article || !state.ready) {
        return;
    }

    const currentSequence = ++sequence;
    state.lastSentSequence = currentSequence;

    frame.contentWindow.postMessage({
        type: 'BLOG_ARTICLE_PREVIEW',
        sequence: currentSequence,
        article: state.article
    }, state.portalOrigin);

    scheduleRenderTimeout(state, currentSequence);
}

function handleMessage(frame, state, event) {
    if (event.source !== frame.contentWindow || event.origin !== state.portalOrigin || !event.data) {
        return;
    }

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_READY') {
        window.clearTimeout(state.unavailableTimer);
        state.ready = true;
        invokeDotNetSafely(state, 'NotifyPreviewReady');
        postLatest(frame);
        return;
    }

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_ACK') {
        if (event.data.sequence !== state.lastSentSequence) {
            return;
        }

        window.clearTimeout(state.renderTimer);
        invokeDotNetSafely(
            state,
            'NotifyPreviewRendered',
            Number(event.data.blockFailureCount || 0));
        return;
    }

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_ERROR') {
        const errorSequence = Number(event.data.sequence || 0);
        if (errorSequence !== 0 && errorSequence !== state.lastSentSequence) {
            return;
        }

        window.clearTimeout(state.renderTimer);
        invokeDotNetSafely(
            state,
            'NotifyPreviewFailed',
            String(event.data.message || 'Unknown preview error'));
    }
}

function scheduleUnavailableCheck(state) {
    window.clearTimeout(state.unavailableTimer);
    state.unavailableTimer = window.setTimeout(() => {
        if (!state.ready) {
            invokeDotNetSafely(state, 'NotifyPreviewUnavailable');
        }
    }, 15000);
}

function scheduleRenderTimeout(state, expectedSequence) {
    window.clearTimeout(state.renderTimer);
    state.renderTimer = window.setTimeout(() => {
        if (state.lastSentSequence === expectedSequence) {
            invokeDotNetSafely(
                state,
                'NotifyPreviewFailed',
                `Preview did not acknowledge render sequence ${expectedSequence} within 15 seconds.`);
        }
    }, 15000);
}

export function connectArticlePreview(frame, dotNetObject, portalOrigin) {
    if (!frame || !dotNetObject || !portalOrigin) {
        return;
    }

    disconnectArticlePreview(frame);

    const state = {
        article: null,
        dotNetObject,
        portalOrigin,
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
        state.ready = false;
        window.clearTimeout(state.renderTimer);
        scheduleUnavailableCheck(state);
    };

    frameStates.set(frame, state);
    window.addEventListener('message', state.messageHandler);
    frame.addEventListener('load', state.loadHandler);
    scheduleUnavailableCheck(state);
}

export function sendArticlePreview(frame, article) {
    const state = frameStates.get(frame);

    if (!state) {
        return;
    }

    state.article = article;
    postLatest(frame);
}

export function disconnectArticlePreview(frame) {
    const state = frameStates.get(frame);

    if (!state) {
        return;
    }

    state.disconnected = true;
    window.clearTimeout(state.unavailableTimer);
    window.clearTimeout(state.renderTimer);
    window.removeEventListener('message', state.messageHandler);
    frame.removeEventListener('load', state.loadHandler);
    frameStates.delete(frame);
}
