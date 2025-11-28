(function () {
	const overlay = document.querySelector('[data-modal-overlay]');
	const closeBtn = document.querySelector('.modal-close');
	const links = document.querySelectorAll('.table .link-primary[data-action="view"]');

	function setText(id, value) {
		const el = document.getElementById(id);
		if (el) el.textContent = value;
	}

	function openModal() {
		overlay.hidden = false;
		overlay.classList.add('is-open');
		closeBtn && closeBtn.focus();
	}

	function closeModal() {
		overlay.classList.remove('is-open');
		overlay.hidden = true;
	}

	links.forEach((link) => {
		link.addEventListener('click', function (e) {
			e.preventDefault();
			const tr = this.closest('tr');
			if (!tr) return;
			const tds = tr.querySelectorAll('td');
			const id = tds[0]?.textContent?.trim() || '';
			const service = tds[1]?.textContent?.trim() || '';
			const requester = tds[2]?.textContent?.trim() || '';
			const owner = tds[3]?.textContent?.trim() || '';
			const statusText = (tds[4]?.innerText || '').trim();
			const date = tds[5]?.textContent?.trim() || '';

			setText('md-id', id);
			setText('md-service', service);
			setText('md-requester', requester);
			setText('md-owner', owner);
			setText('md-date', date);

			const statusEl = document.getElementById('md-status');
			if (statusEl) {
				statusEl.textContent = statusText;
				const isPending = statusText.toLowerCase().includes('pending');
				statusEl.className = 'badge ' + (isPending ? 'badge--pending' : 'badge--completed');
			}

			openModal();
		});
	});

	overlay.addEventListener('click', function (e) {
		if (e.target === overlay) closeModal();
	});
	closeBtn.addEventListener('click', closeModal);
	document.addEventListener('keydown', function (e) {
		if (e.key === 'Escape' && !overlay.hidden) closeModal();
	});
})();