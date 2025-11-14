(() => {
	const accordion = document.querySelector('.accordion');
	if (!accordion) return;

	let activeItem = accordion.querySelector('.accordion-item.active') || null;

	const toggleItem = (item) => {
		if (!item) return;
		if (item.classList.contains('active')) {
			item.classList.remove('active');
			activeItem = null;
			return;
		}
		if (activeItem && activeItem !== item) activeItem.classList.remove('active');
		item.classList.add('active');
		activeItem = item;
	};

	// Use pointerdown for snappier feel; preventDefault to avoid delayed click
	accordion.addEventListener('pointerdown', (e) => {
		const header = e.target.closest('.accordion-header');
		if (!header || !accordion.contains(header)) return;
		e.preventDefault();
		const item = header.closest('.accordion-item');
		toggleItem(item);
	}, { passive: false });

	// Keyboard accessibility (headers are buttons, so Enter/Space should work)
	accordion.addEventListener('keydown', (e) => {
		if (e.key !== 'Enter' && e.key !== ' ') return;
		const header = e.target.closest('.accordion-header');
		if (!header) return;
		e.preventDefault();
		const item = header.closest('.accordion-item');
		toggleItem(item);
	});
})();