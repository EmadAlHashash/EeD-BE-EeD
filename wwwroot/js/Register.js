document.addEventListener('DOMContentLoaded', () => {
	// Particles
	const container = document.querySelector('.particles');
	if (container) {
		const count = 250; // balanced amount
		for (let i = 0; i < count; i++) {
			const p = document.createElement('div');
			p.className = 'gold-particle';
			const size = Math.random() * 2 + 0.8; // ~0.8px to 2.8px
			p.style.width = `${size}px`;
			p.style.height = `${size}px`;
			p.style.left = `${Math.random() * 100}%`;
			p.style.top = `${Math.random() * 100}vh`;
			p.style.animationDuration = `${Math.random() * 10 + 12}s`;
			p.style.animationDelay = `${Math.random() * 8}s`;
			container.appendChild(p);
		}
	}

	// Avatar preview
	const fileInput = document.getElementById('avatar-register');
	const uploadLabel = document.querySelector('.avatar-upload');
	let lastUrl = null;

	if (fileInput && uploadLabel) {
		fileInput.addEventListener('change', (e) => {
			const file = e.target.files && e.target.files[0];
			if (!file || !file.type.startsWith('image/')) return;

			// Cleanup previous object URL
			if (lastUrl) {
				URL.revokeObjectURL(lastUrl);
				lastUrl = null;
			}

			// Preview inside the circle
			const url = URL.createObjectURL(file);
			lastUrl = url;
			uploadLabel.style.backgroundImage = `url('${url}')`;
			uploadLabel.classList.add('has-image');
			uploadLabel.setAttribute('aria-label', 'Change avatar');
		});
	}

});