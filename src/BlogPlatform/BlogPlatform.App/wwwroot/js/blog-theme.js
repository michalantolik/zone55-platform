window.blogPlatformTheme = (() => {
    const storageKey = 'blog-platform-theme';
    const defaultTheme = 'engineering-blue-dark';

    function applyTheme(themeName) {
        const theme = themeName || defaultTheme;
        document.documentElement.dataset.blogTheme = theme;
        localStorage.setItem(storageKey, theme);
    }

    function getCurrentTheme() {
        return localStorage.getItem(storageKey) || defaultTheme;
    }

    function initialize() {
        applyTheme(getCurrentTheme());
    }

    return {
        applyTheme,
        getCurrentTheme,
        initialize
    };
})();

window.blogPlatformTheme.initialize();
