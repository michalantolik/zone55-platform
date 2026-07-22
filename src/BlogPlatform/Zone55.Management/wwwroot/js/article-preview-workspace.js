let sequence = 0;
let listenerRegistered = false;
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
    }, '*');
}

function registerWindowListener() {
    if (listenerRegistered) {
        return;
    }

    listenerRegistered = true;

    window.addEventListener('message', event => {
        if (!event.data || event.data.type !== 'BLOG_ARTICLE_PREVIEW_READY') {
            return;
        }

        for (const frame of document.querySelectorAll('iframe[title="Article preview"]')) {
            if (frame.contentWindow === event.source) {
                postLatest(frame);
                break;
            }
        }
    });
}

export function connectArticlePreview(frame) {
    if (!frame) {
        return;
    }

    registerWindowListener();

    if (!frameStates.has(frame)) {
        frameStates.set(frame, { article: null });
    }

    frame.addEventListener('load', () => {
        postLatest(frame);
        window.setTimeout(() => postLatest(frame), 300);
        window.setTimeout(() => postLatest(frame), 1000);
    });
}

export function sendArticlePreview(frame, article) {
    if (!frame) {
        return;
    }

    registerWindowListener();
    frameStates.set(frame, { article });

    postLatest(frame);
    window.setTimeout(() => postLatest(frame), 300);
    window.setTimeout(() => postLatest(frame), 1000);
}
