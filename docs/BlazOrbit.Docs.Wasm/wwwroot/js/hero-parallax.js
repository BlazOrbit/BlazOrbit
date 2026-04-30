let hero = null;
let rafId = 0;
let targetMx = 0, targetMy = 0;
let curMx = 0, curMy = 0;
let scrollY = 0;
let active = false;

const EASE = 0.08;

function tick() {
    curMx += (targetMx - curMx) * EASE;
    curMy += (targetMy - curMy) * EASE;

    if (hero) {
        hero.style.setProperty('--mx', curMx.toFixed(4));
        hero.style.setProperty('--my', curMy.toFixed(4));
        hero.style.setProperty('--sy', `${scrollY.toFixed(1)}px`);
    }

    if (active) {
        rafId = requestAnimationFrame(tick);
    }
}

function onMove(e) {
    if (!hero) return;
    const r = hero.getBoundingClientRect();
    const cx = r.left + r.width / 2;
    const cy = r.top + r.height / 2;
    targetMx = Math.max(-1, Math.min(1, (e.clientX - cx) / (r.width / 2)));
    targetMy = Math.max(-1, Math.min(1, (e.clientY - cy) / (r.height / 2)));
}

function onLeave() {
    targetMx = 0;
    targetMy = 0;
}

function onScroll() {
    if (!hero) return;
    const r = hero.getBoundingClientRect();
    scrollY = Math.max(0, -r.top);
}

export function attach(el) {
    if (!el) return;
    detach();
    hero = el;
    active = true;
    window.addEventListener('mousemove', onMove, { passive: true });
    hero.addEventListener('mouseleave', onLeave);
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
    rafId = requestAnimationFrame(tick);
}

export function detach() {
    active = false;
    cancelAnimationFrame(rafId);
    if (hero) {
        hero.removeEventListener('mouseleave', onLeave);
    }
    window.removeEventListener('mousemove', onMove);
    window.removeEventListener('scroll', onScroll);
    hero = null;
}
