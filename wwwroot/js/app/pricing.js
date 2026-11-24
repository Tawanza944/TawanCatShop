import { config } from './config.js';
import { postJson } from './http.js';
import { formatBaht, escapeHtml } from './format.js';

/** Asks the server what these lines really cost (prices, coupon, shipping). */
export async function fetchQuote(lines, couponCode, useRewards = false) {
    const { ok, body } = await postJson(config.quoteUrl, { items: lines, couponCode: couponCode || null, useRewards });
    return ok ? body : null;
}

/** Writes a quote into a _ReceiptSummary block. */
export function renderReceipt(scope, quote) {
    const receipt = scope.querySelector('[data-receipt]');
    if (!receipt || !quote) return;

    receipt.querySelector('[data-receipt-subtotal]').textContent = formatBaht(quote.subtotal);
    receipt.querySelector('[data-receipt-shipping]').textContent = quote.shippingFee > 0
        ? formatBaht(quote.shippingFee)
        : quote.shippingWaived > 0 ? 'ฟรี (โค้ดส่งฟรี)' : 'ฟรี';
    receipt.querySelector('[data-receipt-total]').textContent = formatBaht(quote.total);

    const discountRow = receipt.querySelector('[data-receipt-discount-row]');
    discountRow.hidden = !(quote.discount > 0);
    receipt.querySelector('[data-receipt-discount-label]').textContent = quote.couponCode ? `โค้ด ${quote.couponCode}` : 'ส่วนลด';
    receipt.querySelector('[data-receipt-discount]').textContent = `−${formatBaht(quote.discount)}`;
    receipt.querySelector('[data-receipt-reward-row]').hidden = !(quote.rewardDiscount > 0);
    receipt.querySelector('[data-receipt-reward]').textContent = `−${formatBaht(quote.rewardDiscount || 0)}`;
}

/** Shows whether the coupon worked, inside the given scope. */
export function renderCouponMessage(scope, quote, requestedCode) {
    const message = scope.querySelector('[data-coupon-message]');
    if (!message) return;
    if (!requestedCode) {
        message.hidden = true;
        return;
    }
    message.hidden = false;
    message.classList.toggle('is-error', Boolean(quote?.couponError));
    message.innerHTML = quote?.couponError
        ? `${escapeHtml(quote.couponError)} <button type="button" class="lc-link" data-coupon-remove>เอาออก</button>`
        : `ใช้โค้ด <strong>${escapeHtml(quote?.couponCode)}</strong> แล้ว <button type="button" class="lc-link" data-coupon-remove>เอาออก</button>`;
}
