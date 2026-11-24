import { config } from './config.js';
import { escapeHtml } from './format.js';

export function toast(message, tone = 'success') {
    const host = document.querySelector('[data-toasts]');
    if (!host) return;
    const el = document.createElement('div');
    el.className = `lc-toast lc-toast--${tone}`;
    el.innerHTML = `<i class="bi ${tone === 'error' ? 'bi-exclamation-circle' : 'bi-check-circle'}" aria-hidden="true"></i><span>${escapeHtml(message)}</span>`;
    host.appendChild(el);
    setTimeout(() => el.classList.add('is-leaving'), 2600);
    setTimeout(() => el.remove(), 3000);
}

export function askToSignIn(reason) {
    const dialog = document.getElementById('signInPrompt');
    if (!dialog) {
        window.location.href = config.loginUrl;
        return;
    }
    dialog.querySelector('[data-sign-in-reason]').textContent = reason;
    dialog.querySelector('[data-sign-in-link]').setAttribute('href', config.loginUrl);
    bootstrap.Modal.getOrCreateInstance(dialog).show();
}

export function setCount(selector, count) {
    document.querySelectorAll(selector).forEach((badge) => {
        badge.textContent = count > 99 ? '99+' : String(count);
        badge.hidden = count === 0;
    });
}

export const drawer = (id) => bootstrap.Offcanvas.getOrCreateInstance(document.getElementById(id));

export const modal = (id) => bootstrap.Modal.getOrCreateInstance(document.getElementById(id));

/** Reads the product snapshot a card or button carries in data-product (JSON written by Razor). */
export function productFrom(element) {
    const host = element.closest('[data-product]');
    if (!host) return null;
    try {
        return JSON.parse(host.dataset.product);
    } catch {
        return null;
    }
}
