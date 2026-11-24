// Shipping label page: one print button. Kept in a file (no inline script) like the rest of the site.
document.querySelector('[data-print]')?.addEventListener('click', () => window.print());
