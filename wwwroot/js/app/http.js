import { config } from './config.js';

/** POSTs JSON with the anti-forgery token and returns { ok, status, body }. Never throws for HTTP errors. */
export async function postJson(url, payload) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'RequestVerificationToken': config.antiForgeryToken
            },
            body: JSON.stringify(payload)
        });
        const body = await response.json().catch(() => ({}));
        return { ok: response.ok && body.success !== false, status: response.status, body };
    } catch {
        return { ok: false, status: 0, body: { message: 'เชื่อมต่อร้านไม่ได้ ลองใหม่อีกครั้ง' } };
    }
}
