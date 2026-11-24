import { formatBaht } from './format.js';

/*
 * Renders every <canvas data-chart='{"kind","labels","values","label","format"}'> with Chart.js.
 * Colours come from the design tokens so charts follow the yellow theme.
 * kind: "bar" | "line" | "hbar" (horizontal bar).
 */
export function renderCharts(root = document) {
    if (typeof window.Chart === 'undefined') return;

    const css = getComputedStyle(document.documentElement);
    const token = (name, fallback) => css.getPropertyValue(name).trim() || fallback;
    const yellow = token('--lc-yellow-400', '#FFD54F');
    const yellowDeep = token('--lc-yellow-600', '#F2B705');
    const cocoa = token('--lc-cocoa-900', '#2B2118');
    const muted = token('--lc-text-muted', '#7a6a58');
    const grid = token('--lc-border', '#eadfcc');

    Chart.defaults.font.family = token('--lc-font-body', 'Anuphan, sans-serif');
    Chart.defaults.color = muted;

    root.querySelectorAll('canvas[data-chart]').forEach((canvas) => {
        let spec;
        try {
            spec = JSON.parse(canvas.dataset.chart);
        } catch {
            return;
        }

        const money = spec.format === 'baht';
        const format = (v) => (money ? formatBaht(v) : Number(v).toLocaleString('th-TH'));
        const horizontal = spec.kind === 'hbar';
        const line = spec.kind === 'line';
        // Only a time series ending today gets its last bar highlighted; category charts stay one colour.
        const last = spec.labels[spec.labels.length - 1] === 'วันนี้' ? spec.values.length - 1 : -1;

        new Chart(canvas, {
            type: line ? 'line' : 'bar',
            data: {
                labels: spec.labels,
                datasets: [{
                    label: spec.label,
                    data: spec.values,
                    // The newest bar (today) is cocoa so it stands out from the yellow history.
                    backgroundColor: line ? 'rgba(255, 213, 79, .28)' : spec.values.map((_, i) => (i === last && !horizontal ? cocoa : yellow)),
                    borderColor: line ? yellowDeep : 'transparent',
                    borderWidth: line ? 3 : 0,
                    borderRadius: line ? 0 : 10,
                    borderSkipped: false,
                    fill: line,
                    tension: 0.35,
                    pointRadius: 0,
                    pointHoverRadius: 5,
                    pointBackgroundColor: cocoa,
                    maxBarThickness: 38
                }]
            },
            options: {
                indexAxis: horizontal ? 'y' : 'x',
                maintainAspectRatio: false,
                interaction: { intersect: false, mode: 'index' },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: cocoa,
                        padding: 10,
                        cornerRadius: 10,
                        displayColors: false,
                        callbacks: { label: (ctx) => `${spec.label} ${format(ctx.parsed[horizontal ? 'x' : 'y'])}` }
                    }
                },
                scales: {
                    x: {
                        grid: { display: horizontal, color: grid },
                        border: { display: false },
                        beginAtZero: true,
                        ticks: horizontal ? { callback: format } : { maxRotation: 0, autoSkip: true }
                    },
                    y: {
                        grid: { display: !horizontal, color: grid },
                        border: { display: false },
                        beginAtZero: true,
                        ticks: horizontal ? {} : { callback: format, precision: 0 }
                    }
                }
            }
        });
    });
}
