/* Ofogh Air Agency – animation layer. Standalone: does not touch site.js behaviour. */
(function () {
    "use strict";

    if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;

    var root = document.documentElement;
    var isRtl = root.getAttribute("dir") === "rtl";
    var canHover = window.matchMedia && window.matchMedia("(hover: hover) and (pointer: fine)").matches;
    root.classList.add("fx-ready");

    function $all(sel, ctx) { return Array.prototype.slice.call((ctx || document).querySelectorAll(sel)); }

    /* ---------- scroll progress + hero parallax ---------- */
    var bar = document.createElement("div");
    bar.className = "fx-progress";
    document.body.appendChild(bar);
    var photo = document.querySelector(".hero-photo");
    var ticking = false;

    function onScroll() {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(function () {
            var max = document.documentElement.scrollHeight - window.innerHeight;
            bar.style.transform = "scaleX(" + (max > 0 ? Math.min(window.scrollY / max, 1) : 0) + ")";
            if (photo && window.scrollY < 900) photo.style.transform = "translateY(" + window.scrollY * 0.25 + "px) scale(1.08)";
            ticking = false;
        });
    }
    window.addEventListener("scroll", onScroll, { passive: true });
    onScroll();

    /* ---------- hero: headline words, clouds, flight arc ---------- */
    var hero = document.querySelector(".hero");
    if (hero) {
        var h1 = hero.querySelector("h1");
        if (h1) {
            var words = h1.textContent.trim().split(/\s+/);
            h1.setAttribute("aria-label", words.join(" "));
            h1.innerHTML = words.map(function (w, i) {
                return '<span class="fx-word" aria-hidden="true" style="--w:' + i + '"><span>' + w + "</span></span>";
            }).join(" ");
        }

        for (var c = 0; c < 3; c++) {
            var cloud = document.createElement("span");
            cloud.className = "fx-cloud";
            hero.insertBefore(cloud, hero.firstChild);
        }

        var NS = "http://www.w3.org/2000/svg";
        var svg = document.createElementNS(NS, "svg");
        svg.setAttribute("class", "fx-arc");
        svg.setAttribute("viewBox", "0 0 1000 300");
        svg.setAttribute("preserveAspectRatio", "none");
        svg.setAttribute("aria-hidden", "true");
        var path = document.createElementNS(NS, "path");
        path.setAttribute("d", "M-20 270 C 220 280, 300 90, 520 120 S 820 230, 1020 40");
        svg.appendChild(path);
        hero.appendChild(svg);

        var plane = document.createElement("div");
        plane.className = "fx-plane";
        plane.setAttribute("aria-hidden", "true");
        plane.innerHTML = '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M21 16v-2l-8-5V3.5c0-.83-.67-1.5-1.5-1.5S10 2.67 10 3.5V9l-8 5v2l8-2.5V19l-2 1.5V22l3.5-1 3.5 1v-1.5L13 19v-5.5l8 2.5z"/></svg>';
        hero.appendChild(plane);

        var len = path.getTotalLength();
        path.style.strokeDashoffset = "0";
        path.style.clipPath = "inset(0 100% 0 0)";
        var DURATION = 3600, start = null;

        function place(p) {
            var box = hero.getBoundingClientRect();
            var a = path.getPointAtLength(len * p);
            var b = path.getPointAtLength(Math.min(len, len * p + 2));
            var ax = a.x / 1000, bx = b.x / 1000;
            if (isRtl) { ax = 1 - ax; bx = 1 - bx; }
            var x = ax * box.width, y = (a.y / 300) * box.height;
            var dx = (bx - ax) * box.width, dy = ((b.y - a.y) / 300) * box.height;
            var ang = Math.atan2(dy, dx) * 180 / Math.PI;
            plane.style.transform = "translate(" + (x - 17) + "px," + (y - 17) + "px) rotate(" + (ang + 90) + "deg)";
        }

        function fly(ts) {
            if (start === null) start = ts;
            var t = Math.min((ts - start - 500) / DURATION, 1);
            if (t < 0) { plane.style.opacity = "0"; requestAnimationFrame(fly); return; }
            var e = t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2;
            plane.style.opacity = t > 0.96 ? String((1 - t) / 0.04) : "1";
            path.style.clipPath = "inset(0 " + (100 - e * 100) + "% 0 0)";
            place(e);
            if (t < 1) requestAnimationFrame(fly);
        }
        // clip-path runs in path-local x, so mirror it with the svg in RTL (svg is scaleX(-1))
        requestAnimationFrame(fly);
        window.addEventListener("resize", function () { place(1); });
    }

    /* ---------- reveal on scroll ---------- */
    var targets = $all(".section-heading, .destination-card, .feature-card, .stat-item, .faq-item, .search-widget, .tour-body > *");
    targets.forEach(function (el, idx) {
        el.setAttribute("data-fx", "");
        var sibs = el.parentNode ? $all(":scope > [data-fx], :scope > * > [data-fx]", el.parentNode) : [];
        var pos = sibs.indexOf(el);
        el.style.setProperty("--i", String(pos > 0 ? pos % 4 : 0));
        if (el.classList.contains("section-heading")) {
            var rule = document.createElement("i");
            rule.className = "fx-rule";
            el.appendChild(rule);
        }
    });

    if ("IntersectionObserver" in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (en) {
                if (!en.isIntersecting) return;
                var el = en.target;
                el.classList.add("fx-in");
                io.unobserve(el);
                setTimeout(function () { el.classList.add("fx-done"); }, 1200);
            });
        }, { threshold: 0.12, rootMargin: "0px 0px -6% 0px" });
        targets.forEach(function (el) { io.observe(el); });
    } else {
        targets.forEach(function (el) { el.classList.add("fx-in", "fx-done"); });
    }

    /* ---------- pointer effects (mouse only) ---------- */
    if (canHover) {
        $all(".destination-card, .feature-card").forEach(function (card) {
            card.classList.add("fx-tilt");
            card.addEventListener("pointermove", function (e) {
                var r = card.getBoundingClientRect();
                var px = (e.clientX - r.left) / r.width, py = (e.clientY - r.top) / r.height;
                card.style.setProperty("--ry", ((px - 0.5) * 8).toFixed(2) + "deg");
                card.style.setProperty("--rx", ((0.5 - py) * 8).toFixed(2) + "deg");
                card.style.setProperty("--mx", (px * 100).toFixed(1) + "%");
                card.style.setProperty("--my", (py * 100).toFixed(1) + "%");
            });
            card.addEventListener("pointerleave", function () {
                card.style.setProperty("--rx", "0deg");
                card.style.setProperty("--ry", "0deg");
            });
        });

        $all(".btn-brand").forEach(function (btn) {
            btn.addEventListener("pointermove", function (e) {
                var r = btn.getBoundingClientRect();
                btn.style.setProperty("--bx", ((e.clientX - r.left - r.width / 2) * 0.12).toFixed(1) + "px");
                btn.style.setProperty("--by", ((e.clientY - r.top - r.height / 2) * 0.2).toFixed(1) + "px");
            });
            btn.addEventListener("pointerleave", function () {
                btn.style.setProperty("--bx", "0px");
                btn.style.setProperty("--by", "0px");
            });
        });
    }
})();
