import { cart } from './cart.js';
import { config } from './config.js';
import { applyAddressRegion, readAddressRegion } from './address-region.js';
import { postJson } from './http.js';
import { escapeHtml, formatBaht } from './format.js';
import { fetchQuote, renderReceipt, renderCouponMessage } from './pricing.js?v=2';
import { askToSignIn, drawer, modal } from './ui.js';

const dialog = document.getElementById('checkoutDialog');
const form = dialog?.querySelector('[data-checkout-form]');

// "Buy now" checks out one product without touching the cart; otherwise the whole cart is used.
let session = { source: 'cart', lines: [] };
let couponCode = null;
let activeQuote = null;
let requestId = null;
let quoteRevision = 0;
let addressReady;
let changingAddresses = false;
let placingOrder = false;
const addressList = form?.querySelector('[data-saved-addresses]');
const selectedAddress = () => form.querySelector('input[name="savedAddress"]:checked');

export function startCheckout(directProduct = null, qty = 1) {
    if (!config.isAuthenticated) {
        askToSignIn('เข้าสู่ระบบก่อนชำระเงิน สินค้าในตะกร้ายังอยู่ครบ');
        return;
    }
    session = directProduct
        ? { source: 'direct', lines: [{ ...directProduct, qty }] }
        : { source: 'cart', lines: cart.items() };
    if (session.lines.length === 0) return;

    requestId = createRequestId();
    form.elements.paymentSlip.value = '';
    form.elements.useRewards.checked = false;
    couponCode = cart.coupon.get();
    if (selectedAddress()) fillSavedAddress();
    else beginAddress();
    form.querySelector('[data-address-message]').hidden = true;
    hideError();
    drawer('cartDrawer').hide();
    modal('checkoutDialog').show();
    refresh();
}

function createRequestId() {
    if (typeof globalThis.crypto?.randomUUID === 'function') {
        return globalThis.crypto.randomUUID();
    }

    // randomUUID is restricted to secure contexts in some browsers. Keep checkout
    // working on deployed HTTP sites by constructing the same UUID v4 format.
    const bytes = new Uint8Array(16);
    if (typeof globalThis.crypto?.getRandomValues === 'function') {
        globalThis.crypto.getRandomValues(bytes);
    } else {
        for (let i = 0; i < bytes.length; i++) bytes[i] = Math.floor(Math.random() * 256);
    }
    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;

    const hex = [...bytes].map((byte) => byte.toString(16).padStart(2, '0')).join('');
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}

async function refresh() {
    const revision = ++quoteRevision;
    activeQuote = null;
    form.querySelector('[data-checkout-submit]').disabled = true;
    const quoteStatus = form.querySelector('[data-checkout-quote-status]');
    quoteStatus.hidden = false;
    quoteStatus.textContent = 'กำลังคำนวณยอดชำระ…';
    form.querySelector('[data-receipt]').hidden = true;
    const list = form.querySelector('[data-checkout-items]');
    list.innerHTML = session.lines.map((l) => `<li><span>${escapeHtml(l.name)} × ${l.qty}</span><span>${formatBaht(l.price * l.qty)}</span></li>`).join('');

    const quote = await fetchQuote(toRequest(), couponCode, form.elements.useRewards.checked);
    if (revision !== quoteRevision) return;
    if (!quote) {
        quoteStatus.textContent = 'คำนวณยอดไม่สำเร็จ กรุณาลองเปลี่ยนตัวเลือกหรือเปิดหน้าชำระเงินใหม่';
        return;
    }
    activeQuote = quote;
    const groups = quote.lines.reduce((result, line) => {
        if (!result.has(line.shopName)) result.set(line.shopName, []);
        result.get(line.shopName).push(line);
        return result;
    }, new Map());
    list.innerHTML = [...groups].map(([shop, lines]) => '<li><strong>' + escapeHtml(shop ?? '') + '</strong></li>' +
        lines.map(line => '<li><span>' + escapeHtml(line.name) + ' × ' + line.quantity +
            (line.flashSaleId ? ' · Flash Sale' : '') + '</span><span>' + formatBaht(line.lineTotal) + '</span></li>').join('')).join('');
    form.querySelector('[data-reward-balance]').textContent = `(มี ${formatBaht(quote.rewardBalance)})`;
    form.elements.useRewards.disabled = !(quote.rewardBalance > 0);
    renderReceipt(form, quote);
    form.querySelector('[data-receipt]').hidden = false;
    quoteStatus.hidden = true;
    renderCouponMessage(form, quote, couponCode);
    form.querySelector('[data-checkout-submit]').disabled = !quote.canCheckout;
    if (quote.problems.length) showError(quote.problems.join(' · '));
}

const toRequest = () => session.lines.map((l) => ({ productId: l.id, quantity: l.qty }));

function savedRegion(address) {
    return {
        provinceCode: address.dataset.province,
        districtCode: address.dataset.district,
        subdistrictCode: address.dataset.subdistrict,
        postalCode: address.dataset.postal
    };
}

function fillSavedAddress() {
    const selected = selectedAddress();
    const fields = form.querySelector('[data-address-fields]');
    const elements = form.elements;
    fields.hidden = !selected || selected.dataset.regionValid === 'true';
    if (!selected) {
        return;
    }
    elements.addressLabel.value = selected.dataset.label;
    elements.customerName.value = selected.dataset.name;
    elements.customerPhone.value = selected.dataset.phone;
    elements.shippingAddress.value = selected.dataset.address;
    if (fields.hidden) {
        addressReady = null;
        return;
    }
    const { provinceCode, districtCode, subdistrictCode, postalCode } = savedRegion(selected);
    addressReady = applyAddressRegion(form, {
        ProvinceCode: provinceCode, DistrictCode: districtCode,
        SubdistrictCode: subdistrictCode, PostalCode: postalCode
    });
}

function beginAddress() {
    const selected = selectedAddress();
    if (selected) selected.checked = false;
    const elements = form.elements;
    elements.addressLabel.value = '';
    elements.customerName.value = elements.customerName.defaultValue;
    elements.customerPhone.value = elements.customerPhone.defaultValue;
    elements.shippingAddress.value = '';
    addressReady = applyAddressRegion(form);
    form.querySelector('[data-address-fields]').hidden = false;
    elements.addressLabel.focus();
}

async function updateAddresses(removeId) {
    if (changingAddresses || placingOrder) return;
    changingAddresses = true;
    const choices = [...addressList.querySelectorAll('input[name="savedAddress"]')];
    choices.forEach(option => { option.disabled = true; });
    const message = form.querySelector('[data-address-message]');
    message.hidden = true;
    const selectedId = selectedAddress()?.value;
    try {
        await addressReady;
        const elements = form.elements;
        const data = removeId
            ? { id: removeId }
            : {
                id: selectedId || 0,
                label: elements.addressLabel.value.trim(),
                recipientName: elements.customerName.value.trim(),
                phone: elements.customerPhone.value.trim(),
                addressLine: elements.shippingAddress.value.trim(),
                ...readAddressRegion(form)
            };
        if (!removeId) {
            const missing = [
                [!data.label, 'กรอกชื่อที่อยู่ เช่น บ้าน', elements.addressLabel],
                [data.recipientName.length < 2, 'กรอกชื่อผู้รับ', elements.customerName],
                [!/^0\d{9}$/.test(data.phone), 'กรอกเบอร์มือถือ 10 หลักที่ขึ้นต้นด้วย 0', elements.customerPhone],
                [data.addressLine.length < 15, 'กรอกรายละเอียดที่อยู่อย่างน้อย 15 ตัวอักษร', elements.shippingAddress],
                [!data.provinceCode, 'เลือกจังหวัด', elements.ProvinceCode],
                [!data.districtCode, 'เลือกอำเภอหรือเขต', elements.DistrictCode],
                [!data.subdistrictCode, 'เลือกตำบลหรือแขวง', elements.SubdistrictCode],
                [!data.postalCode, 'เลือกรหัสไปรษณีย์', elements.PostalCode]
            ].find(([invalid]) => invalid);
            if (missing) {
                missing[2].focus();
                throw new Error(missing[1]);
            }
        }
        const response = await fetch(removeId ? addressList.dataset.deleteUrl : addressList.dataset.saveUrl, {
            method: 'POST',
            headers: { RequestVerificationToken: config.antiForgeryToken },
            body: new URLSearchParams({ ...data, checkout: 'true' })
        });
        if (response.redirected) throw new Error('กรุณาเข้าสู่ระบบอีกครั้ง');
        if (!response.ok) {
            const body = await response.json().catch(() => ({}));
            throw new Error(body.message || 'จัดการที่อยู่ไม่สำเร็จ ลองใหม่อีกครั้ง');
        }
        addressList.innerHTML = await response.text();
        const nextId = response.headers.get('X-Saved-Address-Id') || (selectedId !== removeId ? selectedId : null);
        for (const option of addressList.querySelectorAll('input[name="savedAddress"]')) {
            option.checked = option.value === nextId;
        }
        if (!removeId || selectedId) fillSavedAddress();
        hideError();
        message.textContent = removeId ? 'ลบที่อยู่แล้ว' : 'บันทึกและเลือกที่อยู่แล้ว';
    } catch (error) {
        message.textContent = error.message || 'จัดการที่อยู่ไม่สำเร็จ ลองใหม่อีกครั้ง';
    } finally {
        choices.forEach(option => { option.disabled = false; });
        changingAddresses = false;
        message.hidden = false;
    }
}

function showError(message) {
    const el = form.querySelector('[data-checkout-error]');
    el.textContent = message;
    el.hidden = false;
}

function hideError() {
    form.querySelector('[data-checkout-error]').hidden = true;
}

async function submit(event) {
    event.preventDefault();
    if (changingAddresses || placingOrder) return;
    placingOrder = true;
    try {
        await submitOrder();
    } finally {
        placingOrder = false;
    }
}

async function submitOrder() {
    hideError();
    if (!selectedAddress()) return showError('เลือกที่อยู่จัดส่ง หรือบันทึกที่อยู่ใหม่ก่อนสั่งซื้อ');
    await addressReady;
    const elements = form.elements;
    const name = elements.customerName.value.trim();
    const phone = elements.customerPhone.value.trim();
    const address = elements.shippingAddress.value.trim();
    const selected = selectedAddress();
    if (!selected) return showError('เลือกที่อยู่จัดส่งก่อนสั่งซื้อ');
    const region = selected.dataset.regionValid === 'true' ? savedRegion(selected) : readAddressRegion(form);

    if (name.length < 2) return showError('กรอกชื่อผู้รับ');
    if (!/^0\d{9}$/.test(phone)) return showError('เบอร์มือถือต้องมี 10 หลักและขึ้นต้นด้วย 0');
    if (Object.values(region).some(value => !value)) return showError('เลือกจังหวัด อำเภอ ตำบล และรหัสไปรษณีย์');
    if (address.length < 15) return showError('ที่อยู่ต้องยาวอย่างน้อย 15 ตัวอักษร');

    const file = elements.paymentSlip.files[0];
    if (!activeQuote) return showError('กรุณารอให้ระบบคำนวณยอด');
    const expectedTotal = activeQuote.total;
    const useRewards = elements.useRewards.checked;
    if (expectedTotal > 0 && (!file || file.size > 2 * 1024 * 1024))
        return showError('แนบสลิป JPG หรือ PNG ไม่เกิน 2 MB');
    const button = form.querySelector('[data-checkout-submit]');
    button.disabled = true;
    button.textContent = 'กำลังสั่งซื้อ…';

    let slip = null;
    try { if (file) slip = await encodeSlip(file); }
    catch {
        button.disabled = false;
        button.textContent = "ยืนยันสั่งซื้อ";
        return showError("อ่านไฟล์สลิปไม่สำเร็จ");
    }

    const { ok, body } = await postJson(config.placeOrderUrl, {
        requestId, expectedTotal, slip,
        useRewards,
        customerName: name,
        customerPhone: phone,
        shippingAddress: address,
        region,
        paymentMethod: elements.paymentMethod.value,
        couponCode,
        items: toRequest()
    });

    button.disabled = false;
    button.textContent = 'ยืนยันสั่งซื้อ';
    if (!ok) {
        await refresh();
        if (activeQuote && activeQuote.total !== expectedTotal) elements.paymentSlip.value = '';
        return showError(body.message || 'สั่งซื้อไม่สำเร็จ ลองใหม่อีกครั้ง');
    }

    if (session.source === 'cart') {
        cart.clear();
    }
    if (session.source === 'cart' || couponCode && couponCode === cart.coupon.get()) cart.coupon.set(null);
    modal('checkoutDialog').hide();
    const placed = document.getElementById('orderPlacedDialog');
    placed.querySelector('[data-placed-order-no]').textContent = body.orderNo;
    placed.querySelector('[data-placed-total]').textContent = formatBaht(body.total);
    if (body.trackingUrl) placed.querySelector('[data-placed-tracking]').setAttribute('href', body.trackingUrl);
    modal('orderPlacedDialog').show();
}

function encodeSlip(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(String(reader.result).split(',')[1]);
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

export function bindCheckout() {
    if (!form) return;
    form.addEventListener('submit', submit);
    form.elements.useRewards.addEventListener('input', () => {
        hideError();
        refresh();
    });
    form.addEventListener('change', (event) => {
        if (event.target.name === 'savedAddress') {
            fillSavedAddress();
            hideError();
        }
    });
    form.addEventListener('click', (event) => {
        const remove = event.target.closest('[data-delete-address]');
        if (event.target.closest('[data-add-address]')) {
            if (!changingAddresses && !placingOrder) beginAddress();
        } else if (event.target.closest('[data-save-address]')) {
            updateAddresses();
        } else if (remove) {
            if (!changingAddresses && !placingOrder && window.confirm('ลบที่อยู่ที่บันทึกไว้นี้หรือไม่')) {
                updateAddresses(remove.dataset.deleteAddress);
            }
        } else if (event.target.closest('[data-coupon-apply]')) {
            const input = form.querySelector('[data-coupon-input]');
            couponCode = input.value.trim().toUpperCase() || null;
            input.value = '';
            if (session.source === 'cart') cart.coupon.set(couponCode);
            refresh();
        } else if (event.target.closest('[data-coupon-remove]')) {
            couponCode = null;
            if (session.source === 'cart') cart.coupon.set(null);
            refresh();
        }
    });
}
