import { config } from './config.js';
import { readJson, writeJson } from './storage.js';

// Wishlist is for signed-in shoppers only (one list per account, kept in this browser).
const KEY = config.userId ? `lovecat:wishlist:u${config.userId}` : null;
const listeners = new Set();
let items = KEY ? readJson(KEY, []).filter((p) => Number.isInteger(p?.id)) : [];

function persist() {
    if (KEY) writeJson(KEY, items);
    listeners.forEach((fn) => fn(items));
}

export const wishlist = {
    enabled: Boolean(KEY),
    items: () => items.map((i) => ({ ...i })),
    has: (id) => items.some((i) => i.id === id),
    toggle(product) {
        if (wishlist.has(product.id)) items = items.filter((i) => i.id !== product.id);
        else items.push({ id: product.id, name: product.name, image: product.image, price: product.price, stock: product.stock });
        persist();
        return wishlist.has(product.id);
    },
    remove(id) {
        items = items.filter((i) => i.id !== id);
        persist();
    },
    onChange(fn) {
        listeners.add(fn);
    }
};
