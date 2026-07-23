let sequence = 0;
const frameStates = new WeakMap();

function postLatest(frame) {
    const state = frameStates.get(frame);

    if (!frame?.contentWindow || !state?.article) {
        return;
    }

    frame.contentWindow.postMessage({
        type: 'BLOG_ARTICLE_PREVIEW',
        sequence: ++sequence,
        article: state.article
    }, state.portalOrigin);
}

function handleMessage(frame, state, event) {
    if (event.source !== frame.contentWindow || event.origin !== state.portalOrigin || !event.data) {
        return;
    }

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_READY') {
        window.clearTimeout(state.unavailableTimer);
        state.dotNetObject.invokeMethodAsync('NotifyPreviewReady');
        postLatest(frame);
        return;
    }

    if (event.data.type === 'BLOG_ARTICLE_PREVIEW_ACK') {
        window.clearTimeout(state.unavailableTimer);
        state.dotNetObject.invokeMethodAsync('NotifyPreviewRendered');
    }
}

function scheduleUnavailableCheck(state) {
    window.clearTimeout(state.unavailableTimer);
    state.unavailableTimer = window.setTimeout(() => {
        state.dotNetObject.invokeMethodAsync('NotifyPreviewUnavailable');
    }, 5000);
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
        messageHandler: null,
        loadHandler: null
    };

    state.messageHandler = event => handleMessage(frame, state, event);
    state.loadHandler = () => {
        scheduleUnavailableCheck(state);
        postLatest(frame);
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

    window.clearTimeout(state.unavailableTimer);
    window.removeEventListener('message', state.messageHandler);
    frame.removeEventListener('load', state.loadHandler);
    frameStates.delete(frame);
}
