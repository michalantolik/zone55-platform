const storageKey = "zone55.theme";

function applyTheme(themeKey) {
    document.documentElement.dataset.theme = themeKey;
    document.documentElement.style.colorScheme = themeKey === "light" ? "light" : "dark";
}

export function initializeTheme(defaultThemeKey) {
    const stored = window.localStorage.getItem(storageKey) || defaultThemeKey;
    applyTheme(stored);
    return stored;
}

export function setTheme(themeKey) {
    window.localStorage.setItem(storageKey, themeKey);
    applyTheme(themeKey);
}
