window.blogPlatformPreview = {
    dotNetObject: null,
    registered: false,

    register: (dotNetObject) => {
        window.blogPlatformPreview.dotNetObject = dotNetObject;

        if (window.blogPlatformPreview.registered) {
            window.blogPlatformPreview.log(
                'WARNING',
                'register called more than once. Existing message listener is kept.');
            return;
        }

        window.blogPlatformPreview.registered = true;

        window.addEventListener('message', async (event) => {
            if (!event.data || event.data.type !== 'BLOG_ARTICLE_PREVIEW') {
                return;
            }

            const sequence = event.data.sequence ?? 0;
            const article = event.data.article;
            const json = JSON.stringify(article);
            const currentDotNetObject = window.blogPlatformPreview.dotNetObject;

            if (!currentDotNetObject) {
                return;
            }

            try {
                await currentDotNetObject.invokeMethodAsync(
                    'LoadPreviewArticle',
                    json,
                    sequence);
            } catch (error) {
                console.error('[LIVE_PREVIEW] LoadPreviewArticle failed.', error);
            }
        });

        window.parent.postMessage({
            type: 'BLOG_ARTICLE_PREVIEW_READY'
        }, '*');
    },

    sendAck: (sequence, title, blockCount) => {
        window.parent.postMessage({
            type: 'BLOG_ARTICLE_PREVIEW_ACK',
            sequence,
            title,
            blockCount
        }, '*');
    },

    log: async (level, message) => {
        console.log(`[LIVE_PREVIEW] ${level}: ${message}`);

        const dotNetObject = window.blogPlatformPreview.dotNetObject;

        if (!dotNetObject) {
            return;
        }

        try {
            await dotNetObject.invokeMethodAsync(
                'LogPreviewDiagnostic',
                level,
                message);
        } catch {
            console.warn('[LIVE_PREVIEW] Failed to send APP preview diagnostic.');
        }
    }
};
