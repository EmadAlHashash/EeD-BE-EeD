(function () {
	const openBtn = document.getElementById('open-notif');
	const popover = document.getElementById('notif-popover');
	const ctaBtn = document.querySelector('.notif__button');
	const notifWrap = openBtn ? openBtn.closest('.notif') : null;
	if (!openBtn || !popover || !notifWrap) return;

	function openPopover() {
		popover.classList.add('is-open');
		popover.removeAttribute('hidden');
		openBtn.setAttribute('aria-expanded', 'true');
	}

	function closePopover() {
		popover.classList.remove('is-open');
		popover.setAttribute('hidden', '');
		openBtn.setAttribute('aria-expanded', 'false');
	}

	// toggle on bell click (open/close)
	openBtn.addEventListener('click', function (e) {
		e.stopPropagation();
		if (popover.classList.contains('is-open')) {
			closePopover();
		} else {
			openPopover();
		}
	});

	// close when clicking outside the notif area
	document.addEventListener('click', function (e) {
		if (!notifWrap.contains(e.target)) {
			closePopover();
		}
	});

	// close with Escape
	document.addEventListener('keydown', function (e) {
		if (e.key === 'Escape') closePopover();
	});

	// optionally close when pressing the CTA button
	if (ctaBtn) {
		ctaBtn.addEventListener('click', closePopover);
	}
})();

// User menu dropdown
(function () {
	const openBtn = document.getElementById('open-user-menu');
	const popover = document.getElementById('user-popover');
	const userWrap = openBtn ? openBtn.closest('.user') : null;
	if (!openBtn || !popover || !userWrap) return;

	function openMenu() {
		popover.classList.add('is-open');
		popover.removeAttribute('hidden');
		openBtn.setAttribute('aria-expanded', 'true');
	}

	function closeMenu() {
		popover.classList.remove('is-open');
		popover.setAttribute('hidden', '');
		openBtn.setAttribute('aria-expanded', 'false');
	}

	openBtn.addEventListener('click', function (e) {
		e.stopPropagation();
		if (popover.classList.contains('is-open')) {
			closeMenu();
		} else {
			openMenu();
		}
	});

	document.addEventListener('click', function (e) {
		if (!userWrap.contains(e.target)) {
			closeMenu();
		}
	});

	document.addEventListener('keydown', function (e) {
		if (e.key === 'Escape') closeMenu();
	});


})();

// Mobile navbar toggle
(function () {
	const toggleBtn = document.getElementById('open-nav');
	const nav = document.getElementById('primary-nav');
	if (!toggleBtn || !nav) return;

	function openNav() {
		nav.classList.add('is-open');
		nav.removeAttribute('hidden');
		toggleBtn.setAttribute('aria-expanded', 'true');
	}

	function closeNav() {
		nav.classList.remove('is-open');
		nav.setAttribute('hidden', '');
		toggleBtn.setAttribute('aria-expanded', 'false');
	}

	// initialize hidden on load for small screens
	if (window.matchMedia('(max-width: 767.98px)').matches) {
		nav.setAttribute('hidden', '');
	}

	toggleBtn.addEventListener('click', function (e) {
		e.stopPropagation();
		if (nav.classList.contains('is-open')) {
			closeNav();
		} else {
			openNav();
		}
	});

	// close on outside click
	document.addEventListener('click', function (e) {
		const header = document.querySelector('.header');
		if (!header) return;
		if (!header.contains(e.target)) closeNav();
	});

	// close with Escape
	document.addEventListener('keydown', function (e) {
		if (e.key === 'Escape') closeNav();
	});

	// auto-close when resizing to desktop
	window.addEventListener('resize', function () {
		if (window.matchMedia('(min-width: 768px)').matches) {
			// ensure visible in desktop without hidden attr
			nav.classList.remove('is-open');
			nav.removeAttribute('hidden');
			toggleBtn.setAttribute('aria-expanded', 'false');
		} else {
			// back to hidden by default on mobile
			if (!nav.classList.contains('is-open')) nav.setAttribute('hidden', '');
		}
	});
})();