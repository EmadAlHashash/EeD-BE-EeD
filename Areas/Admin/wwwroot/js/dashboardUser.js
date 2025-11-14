(function () {
	const openers = document.querySelectorAll('[data-open-modal]');
	const body = document.body;
	let lastOpener = null;
	let addAvatarObjectUrl = null;
	let editAvatarObjectUrl = null;

	function getRequestVerificationToken() {
		// Prefer tokens inside any form (modals include hidden forms with tokens)
		const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
		return tokenInput ? tokenInput.value : '';
	}

	function resetEditAvatar(defaultBg) {
		const avatar = document.getElementById('editAvatar');
		const input = document.getElementById('editAvatarInput');
		if (editAvatarObjectUrl) {
			URL.revokeObjectURL(editAvatarObjectUrl);
			editAvatarObjectUrl = null;
		}
		if (avatar) {
			if (defaultBg) {
				avatar.style.backgroundImage = defaultBg;
				avatar.classList.remove('avatar--placeholder');
			} else {
				avatar.style.backgroundImage = '';
				avatar.classList.add('avatar--placeholder');
			}
		}
		if (input) input.value = '';
	}

	function resetAddAvatar() {
		const avatar = document.getElementById('addAvatar');
		const input = document.getElementById('addAvatarInput');
		if (addAvatarObjectUrl) {
			URL.revokeObjectURL(addAvatarObjectUrl);
			addAvatarObjectUrl = null;
		}
		if (avatar) {
			avatar.style.backgroundImage = '';
			avatar.classList.add('avatar--placeholder');
		}
		if (input) input.value = '';
	}

	function setLoadingEl(buttonEl, isLoading) {
		if (!buttonEl) return;
		const textEl = buttonEl.querySelector('span:not(.material-symbols-outlined)') || buttonEl;
		textEl.dataset.defaultText = textEl.dataset.defaultText || textEl.textContent;
		buttonEl.disabled = !!isLoading;
		buttonEl.classList.toggle('is-loading', !!isLoading);
		textEl.textContent = isLoading ? 'Please wait…' : textEl.dataset.defaultText;
	}

	function setLoading(buttonTextEl, isLoading, defaultText) {
		if (!buttonTextEl) return;
		buttonTextEl.dataset.defaultText = buttonTextEl.dataset.defaultText || defaultText || buttonTextEl.textContent;
		buttonTextEl.textContent = isLoading ? 'Saving…' : buttonTextEl.dataset.defaultText;
	}

	function openModal(target) {
		const modal = document.querySelector(target);
		if (!modal) return;
		modal.classList.add('is-open');
		modal.setAttribute('aria-hidden', 'false');
		body.style.overflow = 'hidden';

		// focus first focusable element in modal
		const dialog = modal.querySelector('.modal__dialog');
		const focusables = dialog ? dialog.querySelectorAll('a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])') : null;
		if (focusables && focusables.length) {
			const first = Array.from(focusables).find(el => el.offsetParent !== null || el.getClientRects().length);
			if (first) first.focus();
		}
	}

	function closeModal(modal) {
		if (!modal) return;
		modal.classList.remove('is-open');
		modal.setAttribute('aria-hidden', 'true');
		body.style.overflow = '';
		if (modal.id === 'addUserModal') {
			resetAddAvatar();
		}
		if (modal.id === 'editUserModal') {
			resetEditAvatar();
		}
	}

	async function postJsonWithForm(url, formData, buttonEl) {
		const token = getRequestVerificationToken();
		if (!token) {
			alert('Security token missing. Please reload the page.');
			return;
		}
		setLoadingEl(buttonEl, true);
		try {
			const res = await fetch(url, {
				method: 'POST',
				headers: {
					'RequestVerificationToken': token,
					'X-CSRF-TOKEN': token
				},
				body: formData
			});
			const ct = res.headers.get('content-type') || '';
			let payload = null;
			if (ct.includes('application/json')) {
				try { payload = await res.json(); } catch { payload = null; }
			} else {
				try {
					const text = await res.text();
					payload = text ? JSON.parse(text) : null;
				} catch { payload = null; }
			}
			if (res.ok) {
				if (!payload || payload.success === undefined || payload.success) {
					const modal = buttonEl.closest('.modal');
					if (modal) closeModal(modal);
					location.reload();
					return;
				}
				// JSON says failed
				alert(payload.message || 'Operation failed');
				return;
			}
			// HTTP not ok
			const msg = payload && payload.message ? payload.message : `Request failed (${res.status})`;
			alert(msg);
		} catch (e) {
			alert('Request failed');
		} finally {
			setLoadingEl(buttonEl, false);
		}
	}

	function bindIframeResponse(iframeId, formId, onSuccess) {
		const iframe = document.getElementById(iframeId);
		const form = document.getElementById(formId);
		if (!iframe || !form) return;
		iframe.addEventListener('load', () => {
			try {
				const doc = iframe.contentDocument || iframe.contentWindow?.document;
				if (!doc) return;
				const text = (doc.body && doc.body.textContent) ? doc.body.textContent.trim() : '';
				if (!text) return;
				let json;
				try { json = JSON.parse(text); } catch { json = null; }
				if (!json) return;
				if (json.success) {
					const modal = form.closest('.modal');
					closeModal(modal);
					// refresh table page-wide for simplicity
					location.reload();
				} else {
					alert(json.message || 'Operation failed');
				}
			} finally {
				// reset loading text
				if (formId === 'addUserForm') setLoading(document.getElementById('addUserSubmitText'), false, 'Add User');
				if (formId === 'editUserForm') setLoading(document.getElementById('editUserSubmitText'), false, 'Save');
			}
		});
	}

	openers.forEach(btn => {
		btn.addEventListener('click', () => {
			const target = btn.getAttribute('data-open-modal');
			lastOpener = btn;
			const action = btn.getAttribute('data-admin-action') || btn.getAttribute('data-user-action');
			const userId = btn.getAttribute('data-user-id') || btn.getAttribute('data-id') || '';

			if (target === '#addUserModal') {
				resetAddAvatar();
			}

			const row = btn.closest('tr');
			const data = { id: userId, name: '', email: '', role: '', status: '', avatar: '' };
			if (row) {
				const nameEl = row.querySelector('.cell--name');
				const emailEl = row.querySelector('td:nth-child(3)');
				const roleEl = row.querySelector('td:nth-child(4) .badge');
				const statusEl = row.querySelector('td:nth-child(5) .badge');
				const avatarEl = row.querySelector('.avatar');
				data.name = nameEl ? nameEl.textContent.trim() : '';
				data.email = emailEl ? emailEl.textContent.trim() : '';
				data.role = roleEl ? roleEl.textContent.trim() : '';
				data.status = statusEl ? statusEl.textContent.trim() : '';
				data.avatar = avatarEl ? avatarEl.style.backgroundImage : '';
			}

			if (action === 'view') {
				const m = document.querySelector('#viewUserModal');
				if (m) {
					const name = m.querySelector('#viewName');
					const email = m.querySelector('#viewEmail');
					const role = m.querySelector('#viewRole');
					const status = m.querySelector('#viewStatus');
					const avatar = m.querySelector('#viewAvatar');
					if (name) name.textContent = data.name;
					if (email) email.textContent = data.email;
					if (role) role.textContent = data.role;
					if (status) status.textContent = data.status;
					if (avatar && data.avatar) avatar.style.backgroundImage = data.avatar;
				}
			} else if (action === 'edit') {
				const f = document.getElementById('editUserForm');
				if (f) {
					const id = f.querySelector('#editId');
					const ef = f.querySelector('#editFullName');
					const ee = f.querySelector('#editEmail');
					const er = f.querySelector('#editRole');
					const es = f.querySelector('#editStatus');
					const en = f.querySelector('#editNameSummary');
					const em = f.querySelector('#editEmailSummary');
					const ea = f.querySelector('#editAvatar');
					resetEditAvatar(data.avatar || '');
					if (id) id.value = data.id || '';
					if (ef) ef.value = data.name || '';
					if (ee) ee.value = data.email || '';
					if (er && data.role) {
						const roleMap = { 'Admin': 'Admin', 'SuperAdmin': 'SuperAdmin', 'User': 'User' };
						er.value = roleMap[data.role] || 'User';
					}
					if (es && data.status) {
						const map = { 'Active': 'Active', 'Inactive': 'Inactive', 'Banned': 'Banned' };
						es.value = map[data.status] || 'Active';
					}
					if (en) en.textContent = data.name || '';
					if (em) em.textContent = data.email || '';
					if (ea) {
						if (data.avatar) {
							ea.style.backgroundImage = data.avatar;
							ea.classList.remove('avatar--placeholder');
						} else {
							ea.style.backgroundImage = '';
							ea.classList.add('avatar--placeholder');
						}
					}
				}
			} else if (action === 'block') {
				const m = document.querySelector('#blockUserModal');
				const n = document.getElementById('blockUserName');
				if (n) n.textContent = data.name || 'this user';
				const confirmBtn = m ? m.querySelector('[data-admin-action="block"]') : null;
				if (confirmBtn) confirmBtn.setAttribute('data-user-id', data.id);
			} else if (action === 'delete') {
				const m = document.querySelector('#deleteUserModal');
				const n = document.getElementById('deleteUserName');
				if (n) n.textContent = data.name || 'this user';
				const confirmBtn = m ? m.querySelector('[data-admin-action="delete"]') : null;
				if (confirmBtn) confirmBtn.setAttribute('data-user-id', data.id);
			}
			openModal(target);
		});
	});

	document.addEventListener('click', async (e) => {
		const closeBtn = e.target.closest('[data-close-modal]');
		if (closeBtn) {
			const modal = closeBtn.closest('.modal');
			closeModal(modal);
			return;
		}

		// Delegated admin actions (block, unblock, delete)
		const actionBtn = e.target.closest('[data-admin-action]');
		if (actionBtn) {
			const action = actionBtn.getAttribute('data-admin-action');
			if (!['block', 'unblock', 'delete'].includes(action)) return;

			// Only perform POST if this is a confirm button inside a modal or an inline unblock (no modal)
			const hasModalTarget = actionBtn.hasAttribute('data-open-modal');
			if (hasModalTarget) return; // opener buttons only open modal

			const userId = actionBtn.getAttribute('data-user-id');
			if (!userId) return;

			const endpoints = {
				block: '/Admin/Users/BlockUser',
				unblock: '/Admin/Users/UnblockUser',
				delete: '/Admin/Users/DeleteUser'
			};
			const url = endpoints[action];
			if (!url) return;

			const formData = new FormData();
			formData.append('userId', userId);

			await postJsonWithForm(url, formData, actionBtn);
		}
	});

	document.addEventListener('keydown', (e) => {
		if (e.key === 'Escape') {
			document.querySelectorAll('.modal.is-open').forEach(m => closeModal(m));
		}
	});

	const addForm = document.getElementById('addUserForm');
	if (addForm) {
		const addAvatar = document.getElementById('addAvatar');
		const addAvatarInput = document.getElementById('addAvatarInput');
		if (addAvatar && addAvatarInput) {
			addAvatar.addEventListener('click', () => addAvatarInput.click());
			addAvatar.addEventListener('keydown', (e) => {
				if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); addAvatarInput.click(); }
			});
			addAvatarInput.addEventListener('change', () => {
				const file = addAvatarInput.files && addAvatarInput.files[0];
				if (!file) return;
				if (addAvatarObjectUrl) URL.revokeObjectURL(addAvatarObjectUrl);
				addAvatarObjectUrl = URL.createObjectURL(file);
				addAvatar.style.backgroundImage = `url("${addAvatarObjectUrl}")`;
				addAvatar.classList.remove('avatar--placeholder');
			});
		}

		const pw = document.getElementById('password');
		const cpw = document.getElementById('confirmPassword');
		const pwError = document.getElementById('pwError');
		const dialog = addForm.closest('.modal')?.querySelector('.modal__dialog');
		const submitText = document.getElementById('addUserSubmitText');

		function clearErrors() {
			pw?.classList.remove('is-invalid');
			cpw?.classList.remove('is-invalid');
			if (pwError) pwError.hidden = true;
		}

		addForm.addEventListener('submit', (e) => {
			clearErrors();
			if (pw && cpw && pw.value !== cpw.value) {
				e.preventDefault();
				pw.classList.add('is-invalid');
				cpw.classList.add('is-invalid');
				if (pwError) pwError.hidden = false;
				if (dialog) {
					dialog.classList.remove('shake');
					void dialog.offsetWidth;
					dialog.classList.add('shake');
				}
				return;
			}
			setLoading(submitText, true, 'Add User');
		});

		bindIframeResponse('addUserTarget', 'addUserForm');
	}

	const editAvatar = document.getElementById('editAvatar');
	const editAvatarInput = document.getElementById('editAvatarInput');
	if (editAvatar && editAvatarInput) {
		editAvatar.addEventListener('click', () => editAvatarInput.click());
		editAvatar.addEventListener('keydown', (e) => {
			if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); editAvatarInput.click(); }
		});
		editAvatarInput.addEventListener('change', () => {
			const file = editAvatarInput.files && editAvatarInput.files[0];
			if (!file) return;
			if (editAvatarObjectUrl) URL.revokeObjectURL(editAvatarObjectUrl);
			editAvatarObjectUrl = URL.createObjectURL(file);
			editAvatar.style.backgroundImage = `url("${editAvatarObjectUrl}")`;
			editAvatar.classList.remove('avatar--placeholder');
		});
	}

	const editForm = document.getElementById('editUserForm');
	if (editForm) {
		const editSubmitText = document.getElementById('editUserSubmitText');
		editForm.addEventListener('submit', () => {
			setLoading(editSubmitText, true, 'Save');
		});
		bindIframeResponse('editUserTarget', 'editUserForm');
	}

	// Keyboard trap in modal
	document.addEventListener('keydown', (e) => {
		if (e.key !== 'Tab') return;
		const modal = document.querySelector('.modal.is-open');
		if (!modal) return;
		const dialog = modal.querySelector('.modal__dialog');
		if (!dialog) return;
		const focusables = Array.from(dialog.querySelectorAll('a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])')).filter(el => (el.offsetParent !== null || el.getClientRects().length));
		if (!focusables.length) return;
		const first = focusables[0];
		const last = focusables[focusables.length - 1];
		const active = document.activeElement;
		if (!e.shiftKey && active === last) { e.preventDefault(); first.focus(); }
		else if (e.shiftKey && active === first) { e.preventDefault(); last.focus(); }
	});
})();