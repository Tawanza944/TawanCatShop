import { cart } from './cart.js';
import { wishlist } from './wishlist.js';
import { config } from './config.js';
import { escapeHtml, formatBaht } from './format.js';
import { fetchQuote, renderReceipt, renderCouponMessage } from './pricing.js?v=2';
import { askToSignIn, setCount } from './ui.js';

const cartDrawer = document.getElementById('cartDrawer');
const wishlistDrawer = document.getElementById('wishlistDrawer');
let quoteTicket = 0;

function lineHtml(item, { withQty }) {
    const image = item.image ? `<img src="${escapeHtml(item.image)}" alt="" loading="lazy" />` : '<span class="lc-cart-line__placeholder"><i class="bi bi-image"></i></span>';
    const qty = withQty
        ? `<div class="lc-qty" role="group" aria-label="จำนวน">
               <button type="button" data-qty-step="-1" aria-label="ลดจำนวน">−</button>
               <span>${item.qty}</span>
               <button type="button" data-qty-step="1" aria-label="เพิ่มจำนวน">+</button>
           </div>`
        : `<button type="button" class="lc-btn lc-btn--secondary lc-btn--sm" data-wishlist-to-cart>ใส่ตะกร้า</button>`;
    return `<li class="lc-cart-line" data-line-id="${item.id}">
        <a class="lc-cart-line__image" href="${config.productUrl(item.id)}">${image}</a>
        <div class="lc-cart-line__info">
            <a href="${config.productUrl(item.id)}">${escapeHtml(item.name)}</a>
            <span class="lc-cart-line__price">${formatBaht(item.price)}${item.flashSaleId ? ' · Flash Sale' : ''}</span>
            ${qty}
        </div>
        <button type="button" class="lc-cart-line__remove" data-line-remove aria-label="ลบ ${escapeHtml(item.name)}"><i class="bi bi-trash3"></i></button>
    </li>`;
}

export async function renderCart() {
    setCount('[data-cart-count]', cart.count());
    if (!cartDrawer) return;

    const ticket = ++quoteTicket;
    const items = cart.items();
    cartDrawer.querySelector('[data-cart-empty]').hidden = items.length > 0;
    cartDrawer.querySelector('[data-cart-foot]').hidden = items.length === 0;
    cartDrawer.querySelector('[data-cart-list]').innerHTML = items.map((i) => lineHtml(i, { withQty: true })).join('');
    if (items.length === 0) return;

    const code = cart.coupon.get();
    const quote = await fetchQuote(cart.toRequest(items), code);
    if (ticket !== quoteTicket || !quote) return;

    const pricedByProduct = new Map(quote.lines.map(line => [line.productId, line]));
    const groups = new Map();
    for (const item of items) {
        const priced = pricedByProduct.get(item.id);
        const shop = priced?.shopName ?? '';
        if (!groups.has(shop)) groups.set(shop, []);
        groups.get(shop).push(priced ? { ...item, price: priced.unitPrice, flashSaleId: priced.flashSaleId } : item);
    }
    cartDrawer.querySelector('[data-cart-list]').innerHTML = [...groups].map(([shop, lines]) =>
        (shop ? '<li class="lc-hint">' + escapeHtml(shop) + '</li>' : '') +
        lines.map(line => lineHtml(line, { withQty: true })).join('')).join('');

    renderReceipt(cartDrawer, quote);
    renderCouponMessage(cartDrawer, quote, code);
    const problems = cartDrawer.querySelector('[data-cart-problems]');
    problems.hidden = quote.problems.length === 0;
    problems.textContent = quote.problems.join(' · ');
    cartDrawer.querySelector('[data-start-checkout]').disabled = !quote.canCheckout;
}

export function renderWishlist() {
    setCount('[data-wishlist-count]', wishlist.items().length);
    if (!wishlistDrawer) return;
    const items = wishlist.items();
    wishlistDrawer.querySelector('[data-wishlist-empty]').hidden = items.length > 0;
    wishlistDrawer.querySelector('[data-wishlist-list]').innerHTML = items.map((i) => lineHtml(i, { withQty: false })).join('');
    document.querySelectorAll('[data-toggle-wishlist]').forEach((button) => {
        const id = Number(button.closest('[data-product]')?.dataset.productId);
        button.setAttribute('aria-pressed', String(wishlist.has(id)));
    });
}

export function bindDrawers() {
    cartDrawer?.addEventListener('click', (event) => {
        const line = event.target.closest('[data-line-id]');
        const id = line ? Number(line.dataset.lineId) : null;
        const step = event.target.closest('[data-qty-step]');
        if (step && id) {
            const current = cart.items().find((i) => i.id === id);
            cart.setQty(id, (current?.qty ?? 0) + Number(step.dataset.qtyStep));
        } else if (event.target.closest('[data-line-remove]') && id) {
            cart.remove(id);
        } else if (event.target.closest('[data-coupon-apply]')) {
            applyCoupon(cartDrawer);
        } else if (event.target.closest('[data-coupon-remove]')) {
            cart.coupon.set(null);
            renderCart();
        }
    });

    wishlistDrawer?.addEventListener('click', (event) => {
        const line = event.target.closest('[data-line-id]');
        const id = line ? Number(line.dataset.lineId) : null;
        if (!id) return;
        if (event.target.closest('[data-line-remove]')) {
            wishlist.remove(id);
        } else if (event.target.closest('[data-wishlist-to-cart]')) {
            const product = wishlist.items().find((i) => i.id === id);
            if (product) cart.add(product, 1);
        }
    });

    cart.onChange(renderCart);
    wishlist.onChange(renderWishlist);
}

function applyCoupon(scope) {
    const input = scope.querySelector('[data-coupon-input]');
    const code = input.value.trim().toUpperCase();
    if (!code) return;
    if (!config.isAuthenticated) {
        askToSignIn('เข้าสู่ระบบก่อนใช้โค้ดส่วนลด');
        return;
    }
    cart.coupon.set(code);
    input.value = '';
    renderCart();
}
