// localStorage can be unavailable (private mode, blocked storage); the shop must still work.
export function readJson(key, fallback) {
    try {
        const raw = localStorage.getItem(key);
        return raw ? JSON.parse(raw) : fallback;
    } catch {
        return fallback;
    }
}

export function writeJson(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
    } catch {
        /* storage full or blocked: keep working in memory */
    }
}

export function remove(key) {
    try {
        localStorage.removeItem(key);
    } catch {
        /* ignore */
    }
}
