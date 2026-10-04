(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var select = document.getElementById('hotelNameSelect');
        var hidden = document.getElementById('name');
        var newWrap = document.getElementById('hotelNameNewWrap');
        var newInput = document.getElementById('hotelNameNew');

        if (!select || !hidden) return;

        function showNewInput(show) {
            if (!newWrap) return;
            newWrap.style.display = show ? '' : 'none';
        }

        function sync() {
            if (select.value === '__new__') {
                showNewInput(true);
                hidden.value = newInput ? newInput.value : '';
            } else {
                showNewInput(false);
                hidden.value = select.value;
            }
        }

        var currentValue = hidden.value || '';
        var matched = false;
        for (var i = 0; i < select.options.length; i++) {
            if (select.options[i].value === currentValue && currentValue !== '') {
                matched = true;
                break;
            }
        }

        if (matched) {
            select.value = currentValue;
        } else if (currentValue) {
            select.value = '__new__';
            showNewInput(true);
            if (newInput) newInput.value = currentValue;
        }

        select.addEventListener('change', sync);
        if (newInput) {
            newInput.addEventListener('input', sync);
        }

        var form = select.closest('form');
        if (form) {
            form.addEventListener('submit', sync);
        }
    });
})();
