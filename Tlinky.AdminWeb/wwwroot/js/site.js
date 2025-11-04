// ==============================
// 💡 Tlinky Admin UI Utilities (Updated)
// ==============================

// --- Lightweight modal ---
window.openModal = function (html, includeClose = false) {
    closeModal(); // ensure only one modal is active

    const overlay = document.createElement('div');
    overlay.id = 'tlinky-modal';
    overlay.style.cssText = `
        position:fixed; inset:0; background:rgba(0,0,0,.4);
        display:flex; align-items:center; justify-content:center; z-index:9999;
        backdrop-filter: blur(2px);
    `;
    overlay.addEventListener('click', e => {
        if (e.target === overlay) closeModal();
    });

    const card = document.createElement('div');
    card.style.cssText = `
        background:#fff; max-width:560px; width:92%; border-radius:16px;
        padding:24px 20px; box-shadow:0 12px 30px rgba(0,0,0,.25);
        animation:modalIn .2s ease; overflow-y:auto; max-height:85vh;
    `;

    // 🧩 Only append Close button if requested (for info-only modals)
    card.innerHTML = html + (includeClose ? `
        <div style="display:flex; justify-content:flex-end; margin-top:16px">
            <button class="btn" onclick="closeModal()">Close</button>
        </div>
    ` : '');

    overlay.appendChild(card);
    document.body.appendChild(overlay);
};

// --- Close modal ---
window.closeModal = function () {
    const el = document.getElementById('tlinky-modal');
    if (el) el.remove();
};

// --- Open modal from <template> ---
window.openModalTemplate = function (templateId) {
    const template = document.getElementById(templateId);
    if (template) {
        openModal(template.innerHTML); // ✅ now uses clean modal (no forced Close button)
    } else {
        console.error(`Template with ID '${templateId}' not found.`);
    }
};

// --- Confirm delete helper ---
window.confirmDelete = function (btn, entity, name) {
    openModal(`
        <h3>Delete ${entity}</h3>
        <p>Are you sure you want to delete <strong>${name}</strong>? This cannot be undone.</p>
        <div style="display:flex; justify-content:flex-end; gap:10px; margin-top:16px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn danger" onclick="(function(){
                const row = (btn.closest && btn.closest('tr')) || null;
                if (row) row.remove();
                closeModal();
                tlinkyToast('${entity} deleted');
            })()">Delete</button>
        </div>
    `);
};

// --- Toast (small notification) ---
window.tlinkyToast = function (msg) {
    const t = document.createElement('div');
    t.style.cssText = `
        position:fixed; bottom:18px; right:18px; background:#111827; color:#fff;
        padding:10px 14px; border-radius:10px;
        font:14px/1.2 system-ui,Segoe UI,Roboto,Arial;
        box-shadow:0 8px 18px rgba(0,0,0,.18);
        z-index:10000; opacity:0; transform:translateY(8px);
        transition:opacity .2s ease, transform .2s ease;
    `;
    t.textContent = msg;
    document.body.appendChild(t);
    requestAnimationFrame(() => {
        t.style.opacity = 1;
        t.style.transform = 'translateY(0)';
    });
    setTimeout(() => {
        t.style.opacity = 0;
        t.style.transform = 'translateY(8px)';
        setTimeout(() => t.remove(), 200);
    }, 2500);
};

// --- Esc key to close modal ---
document.addEventListener('keydown', e => {
    if (e.key === 'Escape') closeModal();
});

// --- Animation keyframes ---
const style = document.createElement('style');
style.textContent = `
@keyframes modalIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: none; }
}
`;
document.head.appendChild(style);
