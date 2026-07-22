let sequence = 0;

export function sendArticlePreview(frame, article) {
    const send = () => frame?.contentWindow?.postMessage({
        type: 'BLOG_ARTICLE_PREVIEW',
        sequence: ++sequence,
        article
    }, '*');

    if (!frame) {
        return;
    }

    send();
    window.setTimeout(send, 250);
}
