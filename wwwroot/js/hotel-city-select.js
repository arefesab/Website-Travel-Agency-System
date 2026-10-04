(function () {
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
