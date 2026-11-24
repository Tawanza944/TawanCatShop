import { renderCharts } from './charts.js';
import { applyAddressRegion } from './address-region.js';

/*
 * Back-office behaviour, all driven by data-* attributes written by Razor:
 *   data-fill-form   on a modal  -> the button that opened it may carry data-fill (JSON) to prefill the form (edit);
 *                                   no data-fill means "add", so the form goes back to its defaults.
 *   data-confirm     on a form   -> ask in the shared #confirmDialog before submitting.
 *   data-auto-submit on a select -> submit its GET filter form when changed.
 *   data-image-input / data-image-preview -> live preview of a product photo.
 */

// ---------- Prefill edit modals ----------
document.querySelectorAll('.modal[data-fill-form]').forEach((modalEl) => {
    const form = modalEl.querySelector('form');
    const heading = modalEl.querySelector('[data-fill-heading]');

    modalEl.addEventListener('show.bs.modal', (event) => {
        const trigger = event.relatedTarget;
        form.reset();
        resetPreview(form);

        let data = null;
        try {
            data = trigger?.dataset.fill ? JSON.parse(trigger.dataset.fill) : null;
        } catch {
            data = null;
        }

        applyAddressRegion(form, data ?? {});
        if (heading) heading.textContent = data && trigger.dataset.fillTitle ? trigger.dataset.fillTitle : heading.dataset.fillHeading;
        modalEl.querySelectorAll('[data-new-only]').forEach((el) => { el.hidden = Boolean(data && data.Id); });

        if (!data) {
            const id = form.querySelector('[name="Id"]');
            if (id) id.value = '0';
            return;
        }

        Object.entries(data).forEach(([key, value]) => fillField(form, key, value));
        modalEl.querySelectorAll('[data-fill-text]').forEach((el) => { el.textContent = data[el.dataset.fillText] ?? ''; });
        modalEl.querySelectorAll('[data-fill-src]').forEach((img) => showPreview(form, data[img.dataset.fillSrc]));
    });
});

function fillField(form, name, value) {
    const fields = form.querySelectorAll(`[name="${CSS.escape(name)}"]`);
    fields.forEach((field) => {
        if (field.type === 'hidden' && fields.length > 1) return; // the "false" twin of a checkbox
        if (field.type === 'file') return;
        if (field.type === 'checkbox') {
            field.checked = Array.isArray(value) ? value.includes(field.value) : value === true || value === 'true';
        } else if (field.type === 'radio') {
            field.checked = String(field.value) === String(value);
        } else {
            field.value = value ?? '';
        }
    });
}

// ---------- Confirm before destructive posts ----------
const confirmEl = document.getElementById('confirmDialog');
let pendingForm = null;

document.addEventListener('submit', (event) => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.dataset.confirm || !confirmEl) return;
    if (form.dataset.confirmed === 'true') return;

    event.preventDefault();
    pendingForm = form;
    confirmEl.querySelector('[data-confirm-message]').textContent =
        'confirmShipment' in form.dataset ? shipmentConfirmMessage(form) : form.dataset.confirm;
    bootstrap.Modal.getOrCreateInstance(confirmEl).show();
});

// Copy buttons (e.g. tracking number on the customer's order card).
document.addEventListener('click', async (event) => {
    const button = event.target.closest('[data-copy-text]');
    if (!button) return;
    const label = button.querySelector('[data-copy-label]');
    try {
        await navigator.clipboard.writeText(button.dataset.copyText);
        if (label) label.textContent = 'คัดลอกแล้ว';
    } catch {
        if (label) label.textContent = 'คัดลอกไม่ได้ กดค้างที่เลขเพื่อคัดลอก';
    }
    if (label) setTimeout(() => { label.textContent = 'คัดลอก'; }, 2000);
});

// Shipment form: repeat exactly what will be saved, because a tracking number cannot be edited after it is sent.
function shipmentConfirmMessage(form) {
    const select = form.querySelector('[data-carrier-select]');
    const other = form.querySelector('[name="otherCarrierName"]').value.trim();
    const carrier = select.value === 'other' ? other : select.selectedOptions[0].text;
    const tracking = form.querySelector('[name="trackingNumber"]').value.replace(/[\s-]+/g, '').toUpperCase();
    return `ส่งพัสดุทาง ${carrier} เลขพัสดุ ${tracking} ใช่ไหม? ยืนยันแล้วแก้ไขเลขพัสดุไม่ได้`;
}

confirmEl?.querySelector('[data-confirm-accept]')?.addEventListener('click', () => {
    if (!pendingForm) return;
    pendingForm.dataset.confirmed = 'true';
    bootstrap.Modal.getOrCreateInstance(confirmEl).hide();
    pendingForm.requestSubmit();
});

// Stop double submits (clicking "หักคอม" twice) once a form is really on its way.
document.addEventListener('submit', (event) => {
    if (event.defaultPrevented) return;
    const button = event.submitter;
    if (button) setTimeout(() => { button.disabled = true; }, 0);
});

// ---------- Filters and photo preview ----------
document.addEventListener('change', (event) => {
    const target = event.target;
    if (target.matches('input[data-auto-upload]') && target.files?.length) {
        target.form?.requestSubmit();
        return;
    }
    if (target.matches('select[data-auto-submit]')) {
        target.form?.requestSubmit();
        return;
    }
    if (target.matches('select[data-carrier-select]')) {
        // "อื่น ๆ" needs a typed carrier name; the server checks it again.
        const field = target.form.querySelector('[data-other-carrier]');
        const isOther = target.value === 'other';
        field.hidden = !isOther;
        field.querySelector('input').required = isOther;
        return;
    }
    if (target.matches('[data-image-input]')) {
        if (target.type === 'file' && target.files?.[0]) {
            showPreview(target.form, URL.createObjectURL(target.files[0]));
        } else if (target.type === 'url' && target.value) {
            showPreview(target.form, target.value);
        }
    }
});

function showPreview(form, src) {
    const img = form?.querySelector('[data-image-preview]');
    if (!img) return;
    img.hidden = !src;
    if (src) img.src = src;
    const empty = form.querySelector('[data-image-empty]');
    if (empty) empty.hidden = Boolean(src);
}

function resetPreview(form) {
    const img = form.querySelector('[data-image-preview]');
    if (!img) return;
    img.hidden = true;
    img.removeAttribute('src');
    const empty = form.querySelector('[data-image-empty]');
    if (empty) empty.hidden = false;
}

renderCharts();
document.addEventListener('change', event => {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.hasAttribute('data-payout-slip-max')) return;
    const tooLarge = input.files[0]?.size > Number(input.dataset.payoutSlipMax);
    input.setCustomValidity(tooLarge ? 'สลิปโอนให้ร้านต้องมีขนาดไม่เกิน 10 MB' : '');
    if (tooLarge) input.reportValidity();
});

