window.zone55Auth = {
    getToken: function (key) {
        return sessionStorage.getItem(key);
    },
    setToken: function (key, token) {
        sessionStorage.setItem(key, token);
    },
    removeToken: function (key) {
        sessionStorage.removeItem(key);
    }
};
