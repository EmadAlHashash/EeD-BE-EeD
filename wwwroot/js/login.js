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
	// Show/Hide password toggle
	const password = document.getElementById("password");
	const toggle = document.getElementById("togglePassword");
	if (password && toggle) {
		toggle.addEventListener("click", (e) => {
			e.preventDefault(); // avoid form submit
			const show = password.getAttribute("type") === "password";
			password.setAttribute("type", show ? "text" : "password");
			toggle.setAttribute("aria-pressed", show ? "true" : "false");
			toggle.setAttribute("aria-label", show ? "Hide password" : "Show password");
		});
	}
	
});