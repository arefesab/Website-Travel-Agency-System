(function () {
    // Photo carousel: arrows, thumbnails, swipe, keyboard, and click-to-enlarge (lightbox).
    // Works with any number of photos; with a single photo only the enlarge feature is active.

    var lightbox = null;

    function ensureLightbox() {
        if (lightbox) return lightbox;
        var el = document.createElement('div');
        el.className = 'pc-lightbox';
        el.setAttribute('role', 'dialog');
        el.setAttribute('aria-modal', 'true');
        el.hidden = true;
        el.innerHTML =
            '<button type="button" class="pc-lb-close" aria-label="Close">&times;</button>' +
            '<button type="button" class="pc-lb-nav pc-lb-prev" aria-label="Previous">&#8249;</button>' +
            '<img class="pc-lb-img" alt="" />' +
            '<button type="button" class="pc-lb-nav pc-lb-next" aria-label="Next">&#8250;</button>' +
            '<div class="pc-lb-count"></div>';
        document.body.appendChild(el);

        var img = el.querySelector('.pc-lb-img');
        var count = el.querySelector('.pc-lb-count');
        var state = { urls: [], i: 0 };

        function render() {
            img.src = state.urls[state.i];
            count.textContent = state.urls.length > 1 ? (state.i + 1) + ' / ' + state.urls.length : '';
            el.classList.toggle('single', state.urls.length < 2);
        }
        function step(d) {
            if (state.urls.length < 2) return;
            state.i = (state.i + d + state.urls.length) % state.urls.length;
            render();
        }
        function close() {
            el.hidden = true;
            document.body.style.overflow = '';
        }

        el.querySelector('.pc-lb-close').addEventListener('click', close);
        el.querySelector('.pc-lb-prev').addEventListener('click', function (e) { e.stopPropagation(); step(-1); });
        el.querySelector('.pc-lb-next').addEventListener('click', function (e) { e.stopPropagation(); step(1); });
        el.addEventListener('click', function (e) { if (e.target === el) close(); });
        document.addEventListener('keydown', function (e) {
            if (el.hidden) return;
            if (e.key === 'Escape') close();
            else if (e.key === 'ArrowLeft') step(-1);
            else if (e.key === 'ArrowRight') step(1);
        });

        lightbox = {
            open: function (urls, i) {
                state.urls = urls; state.i = i;
                render();
                el.hidden = false;
                document.body.style.overflow = 'hidden';
            }
        };
        return lightbox;
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-carousel]').forEach(function (root) {
            var track = root.querySelector('.pc-track');
            var slides = root.querySelectorAll('.pc-slide');
            var total = slides.length;
            if (!track || total < 1) return;

            var thumbs = root.querySelectorAll('.pc-thumb');
            var counter = root.querySelector('.pc-counter');
            var index = 0;

            function urls() {
                return Array.prototype.map.call(slides, function (s) {
                    var im = s.querySelector('img[data-zoom]');
                    return im ? im.getAttribute('src') : null;
                });
            }

            function show(i) {
                index = (i + total) % total;
                track.style.transform = 'translateX(' + (-index * 100) + '%)';
                thumbs.forEach(function (t, k) {
                    var on = k === index;
                    t.classList.toggle('is-active', on);
                    if (on && t.scrollIntoView) t.scrollIntoView({ block: 'nearest', inline: 'center', behavior: 'smooth' });
                });
                if (counter) counter.textContent = (index + 1) + ' / ' + total;
            }

            // enlarge on click (ignore the end of a swipe)
            var downX = null, moved = false;
            root.addEventListener('pointerdown', function (e) {
                if (e.target.closest('.pc-btn, .pc-thumb')) { downX = null; return; }
                downX = e.clientX; moved = false;
            });
            root.addEventListener('pointermove', function (e) {
                if (downX !== null && Math.abs(e.clientX - downX) > 6) moved = true;
            });
            root.addEventListener('pointerup', function (e) {
                if (downX === null) return;
                var dx = e.clientX - downX;
                downX = null;
                if (total > 1 && Math.abs(dx) > 40) { show(dx < 0 ? index + 1 : index - 1); return; }
                if (!moved && e.target.closest('img[data-zoom]')) {
                    var list = urls();
                    if (list[index]) ensureLightbox().open(list.filter(Boolean), Math.max(0, list.filter(Boolean).indexOf(list[index])));
                }
            });
            root.addEventListener('pointercancel', function () { downX = null; });

            if (total < 2) return;

            var prev = root.querySelector('.pc-prev');
            var next = root.querySelector('.pc-next');
            if (prev) prev.addEventListener('click', function () { show(index - 1); });
            if (next) next.addEventListener('click', function () { show(index + 1); });
            thumbs.forEach(function (t) {
                t.addEventListener('click', function () { show(parseInt(t.getAttribute('data-index'), 10) || 0); });
            });
            root.addEventListener('keydown', function (e) {
                if (e.key === 'ArrowLeft') { show(index - 1); e.preventDefault(); }
                else if (e.key === 'ArrowRight') { show(index + 1); e.preventDefault(); }
            });
        });
    });
})();
