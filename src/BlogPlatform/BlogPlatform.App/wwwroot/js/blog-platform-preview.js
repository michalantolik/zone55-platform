window.blogPlatformPreview = {
    dotNetObject: null,
    registered: false,
    parentOrigin: null,
    messageHandler: null,
    errorHandler: null,
    rejectionHandler: null,

    register: (dotNetObject) => {
        window.blogPlatformPreview.dotNetObject = dotNetObject;

        if (!window.blogPlatformPreview.registered) {
            window.blogPlatformPreview.registered = true;

            try {
                if (document.referrer) {
                    window.blogPlatformPreview.parentOrigin = new URL(document.referrer).origin;
                }
            } catch (error) {
                console.warn('[LIVE_PREVIEW] Could not determine parent origin.', error);
            }

            window.blogPlatformPreview.messageHandler = async event => {
                if (!event.data || event.data.type !== 'BLOG_ARTICLE_PREVIEW') return;
                if (window.blogPlatformPreview.parentOrigin && event.origin !== window.blogPlatformPreview.parentOrigin) {
                    console.warn('[LIVE_PREVIEW] Ignored unexpected origin.', event.origin);
                    return;
                }

                window.blogPlatformPreview.parentOrigin = event.origin;
                const sequence = event.data.sequence ?? 0;
                const sessionId = event.data.sessionId || 'unknown';
                const currentDotNetObject = window.blogPlatformPreview.dotNetObject;

                if (!currentDotNetObject) {
                    window.blogPlatformPreview.sendError(sequence, 'Preview message arrived before .NET registration.');
                    return;
                }

                try {
                    await currentDotNetObject.invokeMethodAsync(
                        'LoadPreviewArticle',
                        JSON.stringify(event.data.article),
                        sequence,
                        sessionId);
                } catch (error) {
                    const details = error?.stack || error?.message || String(error);
                    console.error('[LIVE_PREVIEW] LoadPreviewArticle failed.', error);
                    window.blogPlatformPreview.sendError(sequence, details);
                }
            };

            window.blogPlatformPreview.errorHandler = event => {
                const details = `${event.message || 'window error'} at ${event.filename || 'unknown'}:${event.lineno || 0}:${event.colno || 0}`;
                window.blogPlatformPreview.sendError(0, details);
            };

            window.blogPlatformPreview.rejectionHandler = event => {
                const reason = event.reason;
                const details = reason?.stack || reason?.message || String(reason || 'Unhandled promise rejection');
                window.blogPlatformPreview.sendError(0, details);
            };

            window.addEventListener('message', window.blogPlatformPreview.messageHandler);
            window.addEventListener('error', window.blogPlatformPreview.errorHandler);
            window.addEventListener('unhandledrejection', window.blogPlatformPreview.rejectionHandler);
        }

        window.blogPlatformPreview.signalReady();
    },

    unregister: () => {
        window.blogPlatformPreview.dotNetObject = null;

        if (!window.blogPlatformPreview.registered) return;

        if (window.blogPlatformPreview.messageHandler) {
            window.removeEventListener('message', window.blogPlatformPreview.messageHandler);
        }
        if (window.blogPlatformPreview.errorHandler) {
            window.removeEventListener('error', window.blogPlatformPreview.errorHandler);
        }
        if (window.blogPlatformPreview.rejectionHandler) {
            window.removeEventListener('unhandledrejection', window.blogPlatformPreview.rejectionHandler);
        }

        window.blogPlatformPreview.messageHandler = null;
        window.blogPlatformPreview.errorHandler = null;
        window.blogPlatformPreview.rejectionHandler = null;
        window.blogPlatformPreview.registered = false;
    },

    signalReady: () => window.parent.postMessage(
        { type: 'BLOG_ARTICLE_PREVIEW_READY' },
        window.blogPlatformPreview.parentOrigin || '*'),

    sendAck: (sequence, title, blockCount, blockFailureCount) => window.parent.postMessage({
        type: 'BLOG_ARTICLE_PREVIEW_ACK',
        sequence,
        title,
        blockCount,
        blockFailureCount: blockFailureCount || 0
    }, window.blogPlatformPreview.parentOrigin || '*'),

    sendError: (sequence, message) => window.parent.postMessage({
        type: 'BLOG_ARTICLE_PREVIEW_ERROR',
        sequence: sequence || 0,
        message: String(message || 'Unknown preview error').slice(0, 4000)
    }, window.blogPlatformPreview.parentOrigin || '*')
};
