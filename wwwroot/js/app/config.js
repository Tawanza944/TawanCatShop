// Reads app settings rendered by the layout (#lc-app). URLs come from Url.Action, never typed here.
const root = document.getElementById('lc-app');
const data = root ? root.dataset : {};

export const config = Object.freeze({
    isAuthenticated: data.authenticated === 'true',
    userId: data.userId ? Number(data.userId) : null,
    isShopper: data.shopper !== 'false',
    quoteUrl: data.quoteUrl,
    placeOrderUrl: data.placeOrderUrl,
    loginUrl: data.loginUrl,
    productUrl: (id) => (data.productUrl || '').replace('__id__', encodeURIComponent(id)),
    antiForgeryToken: root?.querySelector('input[name="__RequestVerificationToken"]')?.value ?? ''
});
