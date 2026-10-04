(function () {
    var isEn = document.documentElement.lang === 'en';

    // Every <input type="file" data-photo-target="<textarea name>" data-photo-status="<status id>">
    // uploads its files and appends the resulting URLs to that textarea.
    var inputs = document.querySelectorAll('input[type="file"][data-photo-target]');
    Array.prototype.forEach.call(inputs, function (input) {
        var textarea = document.querySelector('textarea[name="' + input.getAttribute('data-photo-target') + '"]');
        var status = document.getElementById(input.getAttribute('data-photo-status'));
        if (!textarea) return;

        input.addEventListener('change', function () {
            var files = input.files;
            if (!files || files.length === 0) return;

            var formData = new FormData();
            for (var i = 0; i < files.length; i++) {
                formData.append('files', files[i]);
            }

            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            var token = tokenInput ? tokenInput.value : '';

            if (status) {
                status.textContent = isEn ? 'Uploading...' : 'در حال آپلود...';
            }

            fetch('/Hotels/UploadPhotos', {
                method: 'POST',
                headers: { 'RequestVerificationToken': token },
                body: formData
            })
                .then(function (response) {
                    return response.json().then(function (data) {
                        return { ok: response.ok, data: data };
                    });
                })
                .then(function (result) {
                    if (!result.ok) {
                        if (status) status.textContent = (result.data && result.data.message) || (isEn ? 'Photo upload failed.' : 'خطا در آپلود عکس.');
                        return;
                    }
                    var urls = result.data.urls || [];
                    var existing = textarea.value.trim();
                    var newLines = urls.join('\n');
                    textarea.value = existing ? (existing + '\n' + newLines) : newLines;
                    textarea.dispatchEvent(new Event('input', { bubbles: true }));
                    if (status) status.textContent = isEn ? (urls.length + ' photo(s) uploaded successfully.') : (urls.length + ' عکس با موفقیت آپلود شد.');
                    input.value = '';
                })
                .catch(function () {
                    if (status) status.textContent = isEn ? 'Could not reach the server while uploading.' : 'خطا در ارتباط با سرور هنگام آپلود.';
                });
        });
    });
})();
