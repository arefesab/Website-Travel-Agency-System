(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var hidden = document.getElementById('meal');
        var checklist = document.getElementById('mealChecklist');
        if (!hidden || !checklist) return;

        var checkboxes = checklist.querySelectorAll('.meal-option');

        function initFromHidden() {
            var selected = (hidden.value || '')
                .split(',')
                .map(function (s) { return s.trim(); })
                .filter(Boolean);

            checkboxes.forEach(function (cb) {
                cb.checked = selected.indexOf(cb.value) !== -1;
            });
        }

        function syncHidden() {
            var values = [];
            checkboxes.forEach(function (cb) {
                if (cb.checked) values.push(cb.value);
            });
            hidden.value = values.join(',');
        }

        checkboxes.forEach(function (cb) {
            cb.addEventListener('change', syncHidden);
        });

        initFromHidden();

        var form = hidden.closest('form');
        if (form) {
            form.addEventListener('submit', syncHidden);
        }
    });
})();
