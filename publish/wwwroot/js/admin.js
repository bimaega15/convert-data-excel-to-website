// Perilaku bersama area admin. Sengaja tanpa framework: halaman Razor di sini
// hanya butuh satu alur konfirmasi hapus yang dipakai semua tabel.
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var modalElement = document.getElementById('confirmDeleteModal');
        if (!modalElement) {
            return;
        }

        var modal = new bootstrap.Modal(modalElement);
        var messageElement = document.getElementById('confirmDeleteMessage');
        var confirmButton = document.getElementById('confirmDeleteButton');
        var pending = null;

        // Delegasi event: baris tabel bisa dirender ulang tanpa perlu memasang
        // ulang listener satu per satu.
        document.addEventListener('click', function (event) {
            var trigger = event.target.closest('[data-delete-url]');
            if (!trigger) {
                return;
            }

            event.preventDefault();

            pending = {
                url: trigger.getAttribute('data-delete-url'),
                id: trigger.getAttribute('data-delete-id')
            };

            messageElement.textContent =
                trigger.getAttribute('data-delete-message') ||
                'Data yang dihapus tidak dapat dikembalikan. Lanjutkan?';

            modal.show();
        });

        confirmButton.addEventListener('click', function () {
            if (!pending) {
                return;
            }

            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            var body = new FormData();
            body.append('id', pending.id);
            if (tokenInput) {
                body.append('__RequestVerificationToken', tokenInput.value);
            }

            confirmButton.disabled = true;

            fetch(pending.url, {
                method: 'POST',
                body: body,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('HTTP ' + response.status);
                    }
                    return response.json();
                })
                .then(function (result) {
                    if (result.success) {
                        // Muat ulang agar daftar, jumlah baris, dan paginasi kembali konsisten.
                        window.location.reload();
                        return;
                    }
                    showError(result.message || 'Penghapusan gagal.');
                })
                .catch(function (error) {
                    showError('Tidak dapat menghubungi server: ' + error.message);
                })
                .finally(function () {
                    confirmButton.disabled = false;
                    modal.hide();
                    pending = null;
                });
        });

        function showError(message) {
            var content = document.querySelector('.admin-content');
            if (!content) {
                window.alert(message);
                return;
            }

            var alert = document.createElement('div');
            alert.className = 'alert alert-danger alert-dismissible fade show';
            alert.setAttribute('role', 'alert');
            alert.textContent = message;

            var close = document.createElement('button');
            close.type = 'button';
            close.className = 'btn-close';
            close.setAttribute('data-bs-dismiss', 'alert');
            close.setAttribute('aria-label', 'Tutup');
            alert.appendChild(close);

            content.prepend(alert);
        }
    });
})();
