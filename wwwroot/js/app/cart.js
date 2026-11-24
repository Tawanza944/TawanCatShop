import { config } from './config.js';
import { readJson, writeJson, remove } from './storage.js';

// The cart lives in the browser. Guests use one key; after sign-in their items move into the user's cart.
// Items keep a display snapshot (name, image, price) but the server re-prices everything at checkout.
const GUEST_KEY = 'lovecat:cart:guest';
const USER_KEY = config.userId ? `lovecat:cart:u${config.userId}` : null;
const COUPON_KEY = config.userId ? `lovecat:coupon:u${config.userId}` : null;
const MAX_QTY = 20;

const listeners = new Set();
let items = load();

function load() {
    const own = readJson(USER_KEY ?? GUEST_KEY, []);
    if (!USER_KEY) return sanitize(own);

    const guest = readJson(GUEST_KEY, []);
    if (guest.length === 0) return sanitize(own);

    // Merge the guest cart into the signed-in cart once, then forget it.
    const merged = sanitize(own);
    for (const line of sanitize(guest)) {
        const existing = merged.find((m) => m.id === line.id);
        if (existing) existing.qty = Math.min(MAX_QTY, existing.qty + line.qty);
        else merged.push(line);
    }
    writeJson(USER_KEY, merged);
    remove(GUEST_KEY);
    return merged;
}

function sanitize(list) {
    return Array.isArray(list)
        ? list.filter((l) => Number.isInteger(l?.id) && l.id > 0).map((l) => ({ ...l, qty: Math.max(1, Math.min(MAX_QTY, Number(l.qty) || 1)) }))
        : [];
}

function persist() {
    writeJson(USER_KEY ?? GUEST_KEY, items);
    listeners.forEach((fn) => fn(items));
}

export const cart = {
    items: () => items.map((i) => ({ ...i })),
    count: () => items.reduce((sum, i) => sum + i.qty, 0),
    isEmpty: () => items.length === 0,
    add(product, qty = 1) {
        const existing = items.find((i) => i.id === product.id);
        const limit = Math.min(MAX_QTY, product.stock ?? MAX_QTY);
        if (existing) existing.qty = Math.min(limit, existing.qty + qty);
        else items.push({ id: product.id, name: product.name, image: product.image, price: product.price, stock: product.stock, qty: Math.min(limit, qty) });
        persist();
    },
    setQty(id, qty) {
        const line = items.find((i) => i.id === id);
        if (!line) return;
        if (qty <= 0) items = items.filter((i) => i.id !== id);
        else line.qty = Math.min(MAX_QTY, line.stock ? Math.min(line.stock, qty) : qty);
        persist();
    },
    remove(id) {
        items = items.filter((i) => i.id !== id);
        persist();
    },
    clear() {
        items = [];
        persist();
    },
    toRequest: (list = items) => list.map((i) => ({ productId: i.id, quantity: i.qty })),
    onChange(fn) {
        listeners.add(fn);
    },
    coupon: {
        get: () => (COUPON_KEY ? readJson(COUPON_KEY, null) : null),
        set: (code) => COUPON_KEY && (code ? writeJson(COUPON_KEY, code) : remove(COUPON_KEY))
    }
};
