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
    document.addEventListener('DOMContentLoaded', function () {
        var nameSelect = document.getElementById('hotelNameSelect');
        var nameHidden = document.getElementById('name');

        var citySelect = document.getElementById('hotelCitySelect');
        var cityHidden = document.getElementById('city');
        var cityNewWrap = document.getElementById('hotelCityNewWrap');
        var cityNewInput = document.getElementById('hotelCityNew');

        var mapEl = document.getElementById('hotelNameCityMap');

        if (!citySelect || !cityHidden) return;

        var nameCityMap = {};
        if (mapEl) {
            try {
                nameCityMap = JSON.parse(mapEl.textContent || '{}');
            } catch (e) {
                nameCityMap = {};
            }
        }

        function showCityNewInput(show) {
            if (!cityNewWrap) return;
            cityNewWrap.style.display = show ? '' : 'none';
        }

        function syncCityHidden() {
            if (citySelect.value === '__new__') {
                showCityNewInput(true);
                cityHidden.value = cityNewInput ? cityNewInput.value : '';
            } else {
                showCityNewInput(false);
                cityHidden.value = citySelect.value;
            }
        }

        function rebuildCityOptions(cities, preferredValue) {
            citySelect.innerHTML = '';

            var placeholder = document.createElement('option');
            placeholder.value = '';
            placeholder.textContent = document.documentElement.lang === 'en' ? '— Select —' : '— انتخاب کنید —';
            citySelect.appendChild(placeholder);

            (cities || []).forEach(function (c) {
                var opt = document.createElement('option');
                opt.value = c;
                opt.textContent = c;
                citySelect.appendChild(opt);
            });

            var newOpt = document.createElement('option');
            newOpt.value = '__new__';
            newOpt.textContent = document.documentElement.lang === 'en' ? '+ Add new city' : '+ افزودن استان جدید';
            citySelect.appendChild(newOpt);

            // English page: show translated labels (the option VALUES stay Persian so saving is unaffected)
            if (isEnglishPage() && (cities || []).length) {
                translateMany(cities).then(function (translated) {
                    var opts = citySelect.options;
                    for (var k = 0; k < opts.length; k++) {
                        var at = (cities || []).indexOf(opts[k].value);
                        if (at !== -1 && translated[at]) opts[k].textContent = translated[at];
                    }
                });
            }

            var value = preferredValue || '';
            if (!value) {
                citySelect.value = '';
                showCityNewInput(false);
                cityHidden.value = '';
                return;
            }

            var found = (cities || []).indexOf(value) !== -1;
            if (found) {
                citySelect.value = value;
                showCityNewInput(false);
                cityHidden.value = value;
            } else {
                citySelect.value = '__new__';
                showCityNewInput(true);
                if (cityNewInput) cityNewInput.value = value;
                cityHidden.value = value;
            }
        }

        function onHotelNameChanged(currentName, preferredCity) {
            var cities = (currentName && nameCityMap[currentName]) ? nameCityMap[currentName].slice() : [];
            var preferred = preferredCity;
            if (!preferred && cities.length > 0) {
                preferred = cities[0];
            }
            rebuildCityOptions(cities, preferred);
        }

        if (nameSelect) {
            nameSelect.addEventListener('change', function () {
                var currentName = (nameSelect.value && nameSelect.value !== '__new__') ? nameSelect.value : '';
                onHotelNameChanged(currentName);
            });
        }

        var initialName = (nameHidden && nameHidden.value) || '';
        var initialCity = cityHidden.value || '';
        onHotelNameChanged(initialName, initialCity);

        citySelect.addEventListener('change', syncCityHidden);
        if (cityNewInput) {
            cityNewInput.addEventListener('input', syncCityHidden);
        }

        var form = citySelect.closest('form');
        if (form) {
            form.addEventListener('submit', syncCityHidden);
        }
    });
})();
