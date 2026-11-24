// The home-page cat follows the mouse with its eyes. Disabled for people who prefer reduced motion.
export function bindMascot() {
    const mascot = document.querySelector('[data-mascot]');
    if (!mascot || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    const pupils = [...mascot.querySelectorAll('[data-pupil]')].map((el) => ({
        el,
        cx: Number(el.getAttribute('cx')),
        cy: Number(el.getAttribute('cy'))
    }));
    const reach = Number(mascot.dataset.reach || 3.5);
    let frame = 0;

    window.addEventListener('pointermove', (event) => {
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            const box = mascot.getBoundingClientRect();
            const viewBox = mascot.viewBox.baseVal;
            const scale = box.width / viewBox.width;
            for (const pupil of pupils) {
                const px = box.left + pupil.cx * scale;
                const py = box.top + pupil.cy * scale;
                const angle = Math.atan2(event.clientY - py, event.clientX - px);
                const distance = Math.min(reach, Math.hypot(event.clientX - px, event.clientY - py) / 40);
                pupil.el.setAttribute('cx', (pupil.cx + Math.cos(angle) * distance).toFixed(2));
                pupil.el.setAttribute('cy', (pupil.cy + Math.sin(angle) * distance).toFixed(2));
            }
        });
    });
}
