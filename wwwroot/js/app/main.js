import { cart } from './cart.js';
import { wishlist } from './wishlist.js';
import { config } from './config.js';
import { bindDrawers, renderCart, renderWishlist } from './drawers.js';
import { bindCheckout, startCheckout } from './checkout.js?v=2';
import { bindMascot } from './mascot.js';
import { askToSignIn, drawer, productFrom, toast } from './ui.js';

const flashTimers = [...document.querySelectorAll('[data-flash-countdown]')];
if (flashTimers.length > 0) {
    const timer = window.setInterval(updateFlashTimers, 1000);
    updateFlashTimers();

    function updateFlashTimers() {
        for (const element of flashTimers) {
            const seconds = Math.max(0, Math.ceil((Date.parse(element.dateTime) - Date.now()) / 1000));
            const hours = Math.floor(seconds / 3600);
            const minutes = Math.floor(seconds / 60) % 60;
            const remainder = seconds % 60;
            element.textContent = seconds > 0
                ? `เหลือ ${hours}:${String(minutes).padStart(2, '0')}:${String(remainder).padStart(2, '0')}`
                : 'สิ้นสุดแล้ว';
            if (seconds > 0) continue;

            const card = element.closest('[data-product]');
            if (card && card.closest('[data-flash-section]')) card.hidden = true;
            if (!card) continue;
            const price = card.querySelector('.lc-price-tag--sale');
            const original = price?.querySelector('.lc-price-tag__was');
            if (original) {
                price.querySelector('.lc-price-tag__now').textContent = original.textContent;
                original.remove();
                price.classList.remove('lc-price-tag--sale');
            }
        }
        const section = document.querySelector('[data-flash-section]');
        if (section && !section.querySelector('[data-product]:not([hidden])')) section.hidden = true;
        if (flashTimers.every(element => Date.parse(element.dateTime) <= Date.now())) window.clearInterval(timer);
    }
}

// One click handler for the whole page (event delegation): buttons declare intent with data-* attributes.
document.addEventListener('click', async (event) => {
    const target = event.target;

    if (target.closest('[data-open-cart]')) {
        drawer('cartDrawer').show();
        return;
    }

    if (target.closest('[data-open-wishlist]')) {
        drawer('wishlistDrawer').show();
        return;
    }

    if (target.closest('[data-start-checkout]')) {
        startCheckout();
        return;
    }

    const addButton = target.closest('[data-add-to-cart]');
    if (addButton) {
        event.preventDefault();
        const product = productFrom(addButton);
        if (!product || product.stock <= 0) return;
        cart.add(product, readQuantity(addButton));
        toast(`ใส่ ${product.name} ลงตะกร้าแล้ว`);
        return;
    }

    const buyButton = target.closest('[data-buy-now]');
    if (buyButton) {
        event.preventDefault();
        const product = productFrom(buyButton);
        if (product && product.stock > 0) startCheckout(product, readQuantity(buyButton));
        return;
    }

    const heart = target.closest('[data-toggle-wishlist]');
    if (heart) {
        event.preventDefault();
        if (!wishlist.enabled) {
            askToSignIn('เข้าสู่ระบบเพื่อเก็บสินค้าไว้ในรายการโปรด');
            return;
        }
        const product = productFrom(heart);
        if (product) toast(wishlist.toggle(product) ? 'เก็บไว้ในรายการโปรดแล้ว' : 'เอาออกจากรายการโปรดแล้ว');
        return;
    }

    const use = target.closest('[data-use-coupon]');
    if (use) {
        if (config.isAuthenticated) cart.coupon.set(use.dataset.useCoupon);
        return;
    }

    const step = target.closest('[data-qty-input-step]');
    if (step) {
        const input = step.closest('[data-qty-picker]')?.querySelector('input');
        if (input) {
            const next = Number(input.value) + Number(step.dataset.qtyInputStep);
            input.value = String(Math.max(1, Math.min(Number(input.max) || 20, next)));
        }
    }
});

// A product page can pick a quantity before adding to cart.
function readQuantity(button) {
    const input = button.closest('[data-product]')?.querySelector('[data-qty-picker] input');
    return input ? Math.max(1, Number(input.value) || 1) : 1;
}

bindDrawers();
bindCheckout();
bindMascot();
renderCart();
renderWishlist();

if (document.body.dataset.openCart === 'true' && !cart.isEmpty()) {
    drawer('cartDrawer').show();
}
