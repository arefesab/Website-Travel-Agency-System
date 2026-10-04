(function () {
    // When the admin picks an existing hotel name, fill the "hotel photos" field with the photos
    // that were entered earlier for that hotel. Room photos are never auto-filled.
    document.addEventListener('DOMContentLoaded', function () {
        var nameSelect = document.getElementById('hotelNameSelect');
        var textarea = document.querySelector('textarea[name="photoUrls"]');
        var mapEl = document.getElementById('hotelNamePhotosMap');
        if (!nameSelect || !textarea) return;

        var photosByName = {};
        if (mapEl) {
            try { photosByName = JSON.parse(mapEl.textContent || '{}'); } catch (e) { photosByName = {}; }
        }

        // The field is replaced only while the admin has not edited it by hand
        var lastAutoValue = textarea.value;
        var touched = false;
        textarea.addEventListener('input', function () {
            touched = textarea.value !== lastAutoValue;
        });

        nameSelect.addEventListener('change', function () {
            var name = (nameSelect.value && nameSelect.value !== '__new__') ? nameSelect.value : '';
            var previous = name && photosByName[name] ? photosByName[name] : '';

            if (touched && textarea.value.trim() !== '') {
                // keep manual edits; only fill when the field is empty
                return;
            }
            textarea.value = previous;
            lastAutoValue = previous;
            touched = false;
        });
    });
})();
