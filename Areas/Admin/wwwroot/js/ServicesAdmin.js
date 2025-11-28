(function () {
    // utility: get anti-forgery token
    function getAntiForgeryToken() {
        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenElement) return tokenElement.value;
        var meta = document.querySelector('meta[name="request-verification-token"]');
        return meta ? meta.getAttribute('content') : null;
    }

    function openModal(id) {
        var modal = document.getElementById(id);
        if (!modal) return;
        modal.classList.add('open');
        modal.removeAttribute('hidden');
        var focusable = modal.querySelector('input, textarea, button');
        if (focusable) setTimeout(function () { focusable.focus(); }, 0);
    }
    function closeModal(id) {
        var modal = document.getElementById(id);
        if (!modal) return;
        modal.classList.remove('open');
        modal.setAttribute('hidden', '');
    }

    document.querySelectorAll('[data-open]').forEach(function (btn) {
        btn.addEventListener('click', function () { openModal(btn.getAttribute('data-open')); });
    });

    document.querySelectorAll('.modal-backdrop').forEach(function (backdrop) {
        backdrop.addEventListener('click', function (e) { if (e.target === backdrop) closeModal(backdrop.id); });
        backdrop.querySelectorAll('[data-close]').forEach(function (c) {
            c.addEventListener('click', function () { closeModal(c.getAttribute('data-close')); });
        });
    });

    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') document.querySelectorAll('.modal-backdrop.open').forEach(function (m) { closeModal(m.id); }); });

    function buildJsonFromForm(form) {
        var obj = {};
        Array.prototype.slice.call(form.elements).forEach(function (el) {
            if (!el.name || el.disabled) return;
            var name = el.name;
            if (el.type === 'checkbox') {
                obj[name] = el.checked;
                return;
            }
            if (el.type === 'radio') {
                if (!el.checked) return;
                obj[name] = el.value;
                return;
            }
            var val = el.value;
            if (/^(id|.*Id|.*ID)$/.test(name) || /^\d+$/.test(val)) {
                var n = Number(val);
                if (!isNaN(n)) { obj[name] = n; return; }
            }
            obj[name] = val;
        });
        return obj;
    }

    function handleResponseAsJsonOrError(response) {
        if (!response.ok) {
            return response.text().then(function (text) {
                throw new Error(text || ('HTTP ' + response.status));
            });
        }
        var ct = response.headers.get('content-type') || '';
        if (ct.indexOf('application/json') === -1) {
            return response.text().then(function (text) {
                try { return JSON.parse(text); } catch (e) { throw new Error(text || 'Invalid JSON response'); }
            });
        }
        return response.json();
    }

    function ajaxForm(form) {
        if (!form) return;
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var url = form.getAttribute('action') || window.location.pathname;

            // If there's a standard antiforgery hidden input, prefer FormData post
            var tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
            var useFormData = !!tokenInput;

            if (useFormData) {
                var fd = new FormData(form);

                // also append id to query as a fallback
                var idVal = fd.get('id');
                if (idVal) {
                    var sep = url.indexOf('?') === -1 ? '?' : '&';
                    url = url + sep + 'id=' + encodeURIComponent(idVal);
                }

                fetch(url, { method: 'POST', body: fd, credentials: 'same-origin' })
                    .then(handleResponseAsJsonOrError)
                    .then(function (json) {
                        if (json && json.success) {
                            var modal = form.closest('.modal-backdrop');
                            if (modal) closeModal(modal.id);
                            window.location.reload();
                        } else {
                            var err = form.querySelector('.error-text');
                            if (err) err.textContent = (json && json.message) ? json.message : 'An error occurred';
                        }
                    }).catch(function (err) {
                        var errEl = form.querySelector('.error-text'); if (errEl) errEl.textContent = err && err.message ? err.message : 'Request failed';
                    });

                return;
            }

            // Otherwise send JSON
            var data = buildJsonFromForm(form);
            var token = getAntiForgeryToken();
            var headers = { 'Content-Type': 'application/json' };
            if (token) headers['RequestVerificationToken'] = token;

            // If id exists, also append to query string so server can read from Request.Query as a fallback
            if (data && (data.id !== undefined && data.id !== null)) {
                var sep = url.indexOf('?') === -1 ? '?' : '&';
                url = url + sep + 'id=' + encodeURIComponent(data.id);
            }

            fetch(url, { method: 'POST', headers: headers, body: JSON.stringify(data), credentials: 'same-origin' })
                .then(handleResponseAsJsonOrError)
                .then(function (json) {
                    if (json && json.success) {
                        var modal = form.closest('.modal-backdrop');
                        if (modal) closeModal(modal.id);
                        window.location.reload();
                    } else {
                        var err = form.querySelector('.error-text');
                        if (err) err.textContent = (json && json.message) ? json.message : 'An error occurred';
                    }
                }).catch(function (err) {
                    var errEl = form.querySelector('.error-text'); if (errEl) errEl.textContent = err && err.message ? err.message : 'Request failed';
                });
        });
    }

    // attach to all admin services forms
    document.querySelectorAll('form').forEach(function (f) {
        var action = f.getAttribute('action') || '';
        if (action.indexOf('/Admin/Services/') !== -1) {
            ajaxForm(f);
        }
    });

})();