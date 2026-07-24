window.blogPlatformPreview = {
    dotNetObject: null,
    registered: false,
    parentOrigin: null,

    register: (dotNetObject) => {
        window.blogPlatformPreview.dotNetObject = dotNetObject;

        if (window.blogPlatformPreview.registered) {
            console.warn('[LIVE_PREVIEW] register called more than once. Existing listener is kept.');
            window.blogPlatformPreview.signalReady();
            return;
        }

        window.blogPlatformPreview.registered = true;

        try {
            if (document.referrer) {
                window.blogPlatformPreview.parentOrigin = new URL(document.referrer).origin;
            }
        } catch (error) {
            console.warn('[LIVE_PREVIEW] Could not determine parent origin from referrer.', error);
        }

        window.addEventListener('message', async (event) => {
            if (!event.data || event.data.type !== 'BLOG_ARTICLE_PREVIEW') {
                return;
            }

            if (window.blogPlatformPreview.parentOrigin && event.origin !== window.blogPlatformPreview.parentOrigin) {
                console.warn('[LIVE_PREVIEW] Ignored message from unexpected origin.', event.origin);
                return;
            }

            window.blogPlatformPreview.parentOrigin = event.origin;

            const sequence = event.data.sequence ?? 0;
            const article = event.data.article;
            const currentDotNetObject = window.blogPlatformPreview.dotNetObject;

            if (!currentDotNetObject) {
                console.warn('[LIVE_PREVIEW] Preview message arrived before .NET registration.');
                return;
            }

            try {
                await currentDotNetObject.invokeMethodAsync(
                    'LoadPreviewArticle',
                    JSON.stringify(article),
                    sequence);
            } catch (error) {
                const details = error?.stack || error?.message || String(error);
                console.error('[LIVE_PREVIEW] LoadPreviewArticle failed.', error);
                window.blogPlatformPreview.sendError(sequence, details);
            }
        });

        window.addEventListener('error', event => {
            const details = `${event.message || 'window error'} at ${event.filename || 'unknown'}:${event.lineno || 0}:${event.colno || 0}`;
            console.error('[LIVE_PREVIEW] Window error.', event.error || details);
            window.blogPlatformPreview.sendError(0, details);
        });

        window.addEventListener('unhandledrejection', event => {
            const reason = event.reason;
            const details = reason?.stack || reason?.message || String(reason || 'Unhandled promise rejection');
            console.error('[LIVE_PREVIEW] Unhandled promise rejection.', reason);
            window.blogPlatformPreview.sendError(0, details);
        });

        window.blogPlatformPreview.signalReady();
    },

    signalReady: () => {
        window.parent.postMessage({
            type: 'BLOG_ARTICLE_PREVIEW_READY'
        }, window.blogPlatformPreview.parentOrigin || '*');
    },

    sendAck: (sequence, title, blockCount, blockFailureCount) => {
        window.parent.postMessage({
            type: 'BLOG_ARTICLE_PREVIEW_ACK',
            sequence,
            title,
            blockCount,
            blockFailureCount: blockFailureCount || 0
        }, window.blogPlatformPreview.parentOrigin || '*');
    },

    sendError: (sequence, message) => {
        window.parent.postMessage({
            type: 'BLOG_ARTICLE_PREVIEW_ERROR',
            sequence: sequence || 0,
            message: String(message || 'Unknown preview error').slice(0, 4000)
        }, window.blogPlatformPreview.parentOrigin || '*');
    }
};
