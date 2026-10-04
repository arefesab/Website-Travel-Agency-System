/* Ofogh – scroll "fresh load" animations. Standalone; pairs with site-reveal.css. */
(function () {
    "use strict";

    var root = document.documentElement;
    if (!root.classList.contains("rv-ready")) return;   // set in <head> only when motion is allowed
    window.__rvInit = true;

    var STAGGER = 60, MAX_STEPS = 3;

    /* selector -> variant */
    var GROUPS = [
        [".search-widget", "up"],
        [".section-heading", "fade"],
        [".hot-card", "up"],
        [".dest-card", "up"],
        [".band-point", "up"]
    ];

    var targets = [];
    GROUPS.forEach(function (g) {
        Array.prototype.forEach.call(document.querySelectorAll(g[0]), function (el) {
            if (el.hasAttribute("data-rv")) return;
            el.setAttribute("data-rv", g[1]);
            targets.push(el);
        });
    });

    var firstBatch = true;
    var isRtl = root.getAttribute("dir") === "rtl";

    var io = new IntersectionObserver(function (entries) {
        var shown = entries.filter(function (e) { return e.isIntersecting; });
        if (!shown.length) return;

        // reading order: row by row, then along the reading direction
        shown.sort(function (a, b) {
            var ra = a.boundingClientRect, rb = b.boundingClientRect;
            var dy = Math.round(ra.top / 40) - Math.round(rb.top / 40);
            if (dy !== 0) return dy;
            return isRtl ? rb.left - ra.left : ra.left - rb.left;
        });

        var base = firstBatch ? 200 : 0;   // let the hero text land first
        firstBatch = false;

        shown.forEach(function (en, i) {
            var el = en.target;
            io.unobserve(el);
            var d = base + Math.min(i, MAX_STEPS) * STAGGER;
            el.style.setProperty("--rv-d", d + "ms");
            // next frame so the hidden state is painted first
            requestAnimationFrame(function () { el.classList.add("rv-in"); });
            // after it has landed, hand the element back to its normal styles / hover transitions
            setTimeout(function () {
                el.removeAttribute("data-rv");
                el.style.removeProperty("--rv-d");
            }, d + 1600);
        });
    }, { threshold: 0.12, rootMargin: "0px 0px -6% 0px" });

    targets.forEach(function (el) { io.observe(el); });
})();
