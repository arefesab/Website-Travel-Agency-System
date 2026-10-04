(function () {
    function toEnglishDigits(value) {
        var persian = "۰۱۲۳۴۵۶۷۸۹";
        var arabic = "٠١٢٣٤٥٦٧٨٩";
        return value.replace(/[۰-۹٠-٩]/g, function (ch) {
            var i = persian.indexOf(ch);
            if (i > -1) return String(i);
            i = arabic.indexOf(ch);
            if (i > -1) return String(i);
            return ch;
        });
    }

    function rawDigits(value) {
        value = toEnglishDigits(value || "");
        return value.replace(/[^\d]/g, "");
    }

    function formatNumber(value) {
        var digits = rawDigits(value);
        if (!digits) return "";
        return digits.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    function initPriceInputs() {
        var inputs = document.querySelectorAll(".price-input");
        inputs.forEach(function (input) {
            input.setAttribute("autocomplete", "off");
            input.setAttribute("inputmode", "numeric");

            if (input.value) {
                input.value = formatNumber(input.value);
            }

            input.addEventListener("input", function () {
                var cursorFromEnd = input.value.length - (input.selectionStart || input.value.length);
                input.value = formatNumber(input.value);
                var pos = input.value.length - cursorFromEnd;
                if (pos >= 0 && pos <= input.value.length) {
                    input.setSelectionRange(pos, pos);
                }
            });

            var form = input.closest("form");
            if (form) {
                form.addEventListener("submit", function () {
                    input.value = rawDigits(input.value);
                });
            }
        });
    }

    document.addEventListener("DOMContentLoaded", initPriceInputs);
})();
