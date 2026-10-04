(function () {
    // When the admin picks an existing hotel name (adding another room to the same hotel),
    // fill description, amenities, address and stars with what was entered earlier for that hotel.
    // A field the admin has already typed into by hand is never overwritten.
    document.addEventListener('DOMContentLoaded', function () {
        var nameSelect = document.getElementById('hotelNameSelect');
        var mapEl = document.getElementById('hotelNameDetailsMap');
        if (!nameSelect || !mapEl) return;

        var detailsByName = {};
        try { detailsByName = JSON.parse(mapEl.textContent || '{}'); } catch (e) { detailsByName = {}; }

        var fields = ['description', 'amenities', 'address', 'star'];
        var state = {};

        function isEmpty(el) {
            var v = (el.value || '').trim();
            return v === '' || (el.id === 'star' && v === '0');
        }

        fields.forEach(function (key) {
            var el = document.getElementById(key);
            if (!el) return;
            var s = state[key] = { el: el, lastAuto: el.value, touched: false };
            el.addEventListener('input', function () {
                s.touched = el.value !== s.lastAuto;
            });
        });

        nameSelect.addEventListener('change', function () {
            var name = (nameSelect.value && nameSelect.value !== '__new__') ? nameSelect.value : '';
            var prev = (name && detailsByName[name]) ? detailsByName[name] : {};

            fields.forEach(function (key) {
                var s = state[key];
                if (!s) return;
                // keep manual edits; only replace when untouched or empty
                if (s.touched && !isEmpty(s.el)) return;

                var value = prev[key];
                value = (value === undefined || value === null) ? '' : String(value);
                s.el.value = value;
                s.lastAuto = value;
                s.touched = false;
            });
        });
    });
})();
