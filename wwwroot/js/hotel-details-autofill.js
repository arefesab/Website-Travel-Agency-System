(function () {
    // ---- fa -> en helper (the site is stored in Persian; Google Website Translator does not translate
    // text that JavaScript puts into options / inputs, so on the English page we translate it ourselves) ----
    var gtCache = {};
    function isEnglishPage() {
        return /(?:^|;\s*)site-lang=en(?:;|$)/.test(document.cookie);
    }
    function gtRequest(text) {
        var base = 'https://translate.googleapis.com/translate_a/single?client=gtx&sl=fa&tl=en&dt=t';
        var p = encodeURIComponent(text).length < 1500
            ? fetch(base + '&q=' + encodeURIComponent(text))
            : fetch(base, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' }, body: 'q=' + encodeURIComponent(text) });
        return p.then(function (r) { if (!r.ok) throw new Error('translate failed ' + r.status); return r.json(); })
            .then(function (d) { return (d[0] || []).map(function (seg) { return seg[0] || ''; }).join(''); });
    }
    // Translates an array of strings; resolves with an array of the same length (original text on failure).
    function translateMany(items) {
        var idx = [], texts = [];
        items.forEach(function (t, i) {
            if (gtCache[t] !== undefined) return;
            if (t && t.trim() && /[\u0600-\u06FF]/.test(t) && idx.indexOf(i) === -1 && texts.indexOf(t) === -1) { idx.push(i); texts.push(t); }
        });
        var work = texts.length === 0 ? Promise.resolve() :
            gtRequest(texts.join('\n')).then(function (out) {
                var parts = out.replace(/\r/g, '').split('\n');
                if (parts.length !== texts.length) throw new Error('line count mismatch');
                texts.forEach(function (t, k) { if (parts[k].trim()) gtCache[t] = parts[k].trim(); });
            }).catch(function () {
                // fall back to one request per item
                return Promise.all(texts.map(function (t) {
                    return gtRequest(t).then(function (o) { if (o.trim()) gtCache[t] = o.trim(); }).catch(function (e) { console.warn('Translation failed:', e); });
                }));
            });
        return work.then(function () {
            return items.map(function (t) { return gtCache[t] !== undefined ? gtCache[t] : t; });
        });
    }
    // When the admin picks an existing hotel name (adding another room to the same hotel),
    // fill description, amenities, address and stars with what was entered earlier for that hotel.
    // A field the admin has already typed into by hand is never overwritten.
    //
    // The site is stored in Persian and shown in English through Google Website Translator, which
    // only translates text nodes - NOT values that JavaScript puts into inputs/textareas. So when the
    // page is in English we translate the auto-filled text (fa -> en) ourselves before showing it.
    document.addEventListener('DOMContentLoaded', function () {
        var nameSelect = document.getElementById('hotelNameSelect');
        var mapEl = document.getElementById('hotelNameDetailsMap');
        if (!nameSelect || !mapEl) return;

        // true  => if the admin did not edit an auto-filled (translated) field, the original Persian text
        //          is what gets saved, so the stored data stays Persian (the site's source language).
        // false => whatever is visible in the field (English) is saved as is.
        var SAVE_ORIGINAL_ON_SUBMIT = true;

        var detailsByName = {};
        try { detailsByName = JSON.parse(mapEl.textContent || '{}'); } catch (e) { detailsByName = {}; }

        var fields = ['description', 'amenities', 'address', 'star'];
        var textFields = { description: true, amenities: true, address: true };
        var state = {};
        var requestId = 0;
        var cache = {};

        function isEmpty(el) {
            var v = (el.value || '').trim();
            return v === '' || (el.id === 'star' && v === '0');
        }

        // Translate line by line (batched into one request) so multi-line fields keep one item per line.
        function translateText(text) {
            return translateMany(String(text).split(/\r?\n/)).then(function (lines) { return lines.join('\n'); });
        }

        fields.forEach(function (key) {
            var el = document.getElementById(key);
            if (!el) return;
            var s = state[key] = { el: el, lastAuto: el.value, touched: false, original: null };
            el.addEventListener('input', function () {
                s.touched = el.value !== s.lastAuto;
            });
        });

        function applyValue(s, original, shown) {
            s.el.value = shown;
            s.lastAuto = s.el.value; // the browser normalises line breaks, so read it back
            s.touched = false;
            s.original = (shown !== original) ? original : null;
        }

        nameSelect.addEventListener('change', function () {
            var name = (nameSelect.value && nameSelect.value !== '__new__') ? nameSelect.value : '';
            var prev = (name && detailsByName[name]) ? detailsByName[name] : {};
            var myRequest = ++requestId;
            var english = isEnglishPage();

            fields.forEach(function (key) {
                var s = state[key];
                if (!s) return;
                // keep manual edits; only replace when untouched or empty
                if (s.touched && !isEmpty(s.el)) return;

                var value = prev[key];
                value = (value === undefined || value === null) ? '' : String(value);

                if (!english || !textFields[key] || value === '') {
                    applyValue(s, value, value);
                    return;
                }

                // English page: show the Persian text right away, then swap in the translation.
                applyValue(s, value, value);
                translateText(value).then(function (translated) {
                    // ignore if the admin picked another hotel or edited the field meanwhile
                    if (myRequest !== requestId || s.touched || s.el.value !== s.lastAuto) return;
                    applyValue(s, value, translated);
                });
            });
        });

        var form = nameSelect.closest('form');
        if (form && SAVE_ORIGINAL_ON_SUBMIT) {
            form.addEventListener('submit', function () {
                fields.forEach(function (key) {
                    var s = state[key];
                    if (s && s.original !== null && !s.touched && s.el.value === s.lastAuto) {
                        s.el.value = s.original;
                    }
                });
            });
        }
    });
})();
