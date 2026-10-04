
(function () {
    "use strict";

    var THEME_KEY = "ofogh-theme";
    var root = document.documentElement;

    function applyTheme(theme) {
        if (theme === "dark") {
            root.setAttribute("data-theme", "dark");
        } else {
            root.removeAttribute("data-theme");
        }
    }

    var savedTheme = null;
    try { savedTheme = localStorage.getItem(THEME_KEY); } catch (e) { /* storage may be blocked */ }
    if (savedTheme) applyTheme(savedTheme);

    document.addEventListener("click", function (e) {
        var toggle = e.target.closest(".theme-toggle");
        if (!toggle) return;
        var isDark = root.getAttribute("data-theme") === "dark";
        var next = isDark ? "light" : "dark";
        applyTheme(next);
        try { localStorage.setItem(THEME_KEY, next); } catch (err) { /* ignore */ }
    });

    var NO_PHOTO_ICON = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="16" rx="2.5"/><circle cx="9" cy="10" r="1.6"/><path d="M21 16l-5-5-8 9"/></svg>';
    function replaceWithNoPhoto(img) {
        if (!img || !img.parentNode || img.dataset.noPhotoDone) return;
        img.dataset.noPhotoDone = "1";
        var text = img.getAttribute("data-nophoto-text") || "No photo";
        var box = document.createElement("div");
        box.className = "no-photo" + (img.classList.contains("gallery-main") ? " no-photo-lg" : "");
        box.setAttribute("role", "img");
        box.setAttribute("aria-label", text);
        box.innerHTML = NO_PHOTO_ICON + "<span></span>";
        box.querySelector("span").textContent = text;
        img.parentNode.replaceChild(box, img);
    }
    document.addEventListener("error", function (e) {
        var el = e.target;
        if (el && el.tagName === "IMG" && el.classList.contains("hotel-photo")) replaceWithNoPhoto(el);
    }, true);
    document.querySelectorAll("img.hotel-photo").forEach(function (img) {
        if (img.complete && img.naturalWidth === 0) replaceWithNoPhoto(img);
    });

    var header = document.querySelector(".site-header");
    if (header) {
        var onScroll = function () {
            if (window.scrollY > 12) header.classList.add("is-scrolled");
            else header.classList.remove("is-scrolled");
        };
        onScroll();
        window.addEventListener("scroll", onScroll, { passive: true });
    }

    var tabs = document.querySelectorAll(".search-tab");
    tabs.forEach(function (tab) {
        tab.addEventListener("click", function () {
            var target = tab.getAttribute("data-target");
            document.querySelectorAll(".search-tab").forEach(function (t) { t.classList.remove("is-active"); });
            document.querySelectorAll(".search-panel").forEach(function (p) { p.classList.remove("is-active"); });
            tab.classList.add("is-active");
            var panel = document.querySelector(target);
            if (panel) panel.classList.add("is-active");
        });
    });

    var pills = document.querySelectorAll(".filter-pill");
    var destinationCards = document.querySelectorAll("[data-destination-type]");
    pills.forEach(function (pill) {
        pill.addEventListener("click", function () {
            pills.forEach(function (p) { p.classList.remove("is-active"); });
            pill.classList.add("is-active");
            var filter = pill.getAttribute("data-filter");
            destinationCards.forEach(function (card) {
                var type = card.getAttribute("data-destination-type");
                var show = filter === "all" || filter === type;
                card.style.display = show ? "" : "none";
            });
        });
    });

    var statEls = document.querySelectorAll(".stat-number[data-count-to]");
    var siteLocale = (document.documentElement.lang === "en") ? "en-US" : "fa-IR";
    if (statEls.length && "IntersectionObserver" in window) {
        var animateCount = function (el) {
            var target = parseInt(el.getAttribute("data-count-to"), 10) || 0;
            var suffix = el.getAttribute("data-suffix") || "";
            var duration = 1200;
            var start = null;

            function step(timestamp) {
                if (!start) start = timestamp;
                var progress = Math.min((timestamp - start) / duration, 1);
                var eased = 1 - Math.pow(1 - progress, 3);
                el.textContent = Math.floor(eased * target).toLocaleString(siteLocale) + suffix;
                if (progress < 1) requestAnimationFrame(step);
                else el.textContent = target.toLocaleString(siteLocale) + suffix;
            }
            requestAnimationFrame(step);
        };

        var statObserver = new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    animateCount(entry.target);
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.4 });

        statEls.forEach(function (el) { statObserver.observe(el); });
    }

    var track = document.querySelector(".testimonial-track");
    var dotsWrap = document.querySelector(".testimonial-dots");
    if (track && dotsWrap) {
        var cards = track.querySelectorAll(".testimonial-card");
        var dots = dotsWrap.querySelectorAll("button");
        var setActiveDot = function (index) {
            dots.forEach(function (d, i) { d.classList.toggle("is-active", i === index); });
        };
        dots.forEach(function (dot, i) {
            dot.addEventListener("click", function () {
                var card = cards[i];
                if (card) track.scrollTo({ left: card.offsetLeft - track.offsetLeft, behavior: "smooth" });
            });
        });
        if ("IntersectionObserver" in window) {
            var cardObserver = new IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        var idx = Array.prototype.indexOf.call(cards, entry.target);
                        if (idx > -1) setActiveDot(idx);
                    }
                });
            }, { root: track, threshold: 0.6 });
            cards.forEach(function (c) { cardObserver.observe(c); });
        }
    }

    var backToTop = document.querySelector(".back-to-top");
    if (backToTop) {
        window.addEventListener("scroll", function () {
            backToTop.classList.toggle("is-visible", window.scrollY > 420);
        }, { passive: true });
        backToTop.addEventListener("click", function () {
            window.scrollTo({ top: 0, behavior: "smooth" });
        });
    }

    var listSearch = document.querySelector(".list-search");
    if (listSearch) {
        var rows = document.querySelectorAll(".table-wrap tbody tr");
        var countEl = document.querySelector(".list-count");
        listSearch.addEventListener("input", function () {
            var q = listSearch.value.trim().toLowerCase();
            var visible = 0;
            rows.forEach(function (row) {
                var match = row.textContent.toLowerCase().indexOf(q) > -1;
                row.classList.toggle("is-hidden", !match);
                if (match) visible++;
            });
            if (countEl) {
                var foundText = (document.documentElement.lang === "en") ? " results found" : " مورد یافت شد";
                countEl.textContent = visible.toLocaleString(siteLocale) + foundText;
            }
        });
    }

    var newsletterForm = document.querySelector(".newsletter-form");
    if (newsletterForm) {
        newsletterForm.addEventListener("submit", function (e) {
            e.preventDefault();
            var input = newsletterForm.querySelector("input[type='email']");
            var isEn = document.documentElement.lang === "en";
            var okMsg = isEn ? "Subscribed successfully \u2708" : "با موفقیت در خبرنامه ثبت شدید ✈";
            var errMsg = isEn ? "Please enter your email" : "لطفاً ایمیل خود را وارد کنید";
            showToast(input && input.value ? okMsg : errMsg);
            if (input) input.value = "";
        });
    }

    function showToast(message) {
        var stack = document.querySelector(".toast-stack");
        if (!stack) {
            stack = document.createElement("div");
            stack.className = "toast-stack";
            document.body.appendChild(stack);
        }
        var chip = document.createElement("div");
        chip.className = "toast-chip";
        chip.textContent = message;
        stack.appendChild(chip);
        setTimeout(function () {
            chip.style.opacity = "0";
            chip.style.transition = "opacity .25s ease";
            setTimeout(function () { chip.remove(); }, 260);
        }, 3200);
    }
    window.ofoghShowToast = showToast;

    // Shown after the Google translation could not be loaded and the page fell back to Persian
    try {
        if (sessionStorage.getItem("translate-failed")) {
            sessionStorage.removeItem("translate-failed");
            showToast("ترجمه‌ی انگلیسی در دسترس نیست (اتصال به Google Translate برقرار نشد). لطفاً بعداً دوباره تلاش کنید.");
        }
    } catch (e) {}
})();

// Departures board: rows flip in once, the first time the board scrolls into view
document.addEventListener("DOMContentLoaded", function () {
    var boards = document.querySelectorAll("[data-board]");
    if (!boards.length || !("IntersectionObserver" in window)) return;
    var boardObserver = new IntersectionObserver(function (entries, obs) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("is-in");
                obs.unobserve(entry.target);
            }
        });
    }, { threshold: 0.25 });
    boards.forEach(function (b) {
        b.classList.add("is-armed");
        boardObserver.observe(b);
    });
});
