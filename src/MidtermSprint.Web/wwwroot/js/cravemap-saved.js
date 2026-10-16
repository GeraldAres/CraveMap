window.cravemapSaved = Object.freeze({
    get(key) {
        return window.localStorage.getItem(key);
    },
    set(key, value) {
        window.localStorage.setItem(key, value);
    }
});
