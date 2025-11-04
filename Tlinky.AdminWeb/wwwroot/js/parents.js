// ======================================
// 👨‍👩‍👧 Tlinky Parents CRUD (Neon-linked)
// ======================================

// ---------- INIT ----------
document.addEventListener("DOMContentLoaded", async () => {
    await loadParents();

    const btn = document.getElementById("btnAddParent");
    if (btn) {
        btn.addEventListener("click", async () => {
            await openAddParentModal();
        });
    }
});

// ---------- Load all ----------
async function loadParents() {
    const tbody = document.getElementById("parentsTableBody");
    tbody.innerHTML = `<tr><td colspan="5">Loading...</td></tr>`;

    try {
        const res = await fetch("/Parents/GetAll");
        if (!res.ok) throw new Error("Failed to fetch parents");
        const data = await res.json();

        tbody.innerHTML = "";

        if (!data.length) {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center">No parents found.</td></tr>`;
            return;
        }

        data.forEach(p => {
            tbody.innerHTML += `
            <tr>
                <td>${p.fullName}</td>
                <td>${p.email}</td>
                <td>${p.children}</td>
                <td>
                    <select class="search" onchange="updateParentStatus(${p.parentId}, this.value)">
                        <option ${p.status === "Active" ? "selected" : ""}>Active</option>
                        <option ${p.status === "Suspended" ? "selected" : ""}>Suspended</option>
                    </select>
                </td>
                <td>
                    <button class="btn" onclick="openEditParentModal(${p.parentId})">Edit</button>
                    <button class="btn" onclick="openResetPasswordModal(${p.parentId})">Reset Password</button>
                    <button class="btn danger" onclick="deleteParent(${p.parentId})">Delete</button>
                </td>
            </tr>`;
        });
    } catch (err) {
        console.error("Error loading parents:", err);
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">Error loading parents</td></tr>`;
    }
}

// ---------- Add ----------
async function openAddParentModal() {
    try {
        const res = await fetch("/Parents/Dropdowns");
        if (!res.ok) throw new Error("Dropdown fetch failed");
        const { children } = await res.json();

        const childOptions = children.length
            ? children.map(c => `<option value="${c.childId}">${c.fullName}</option>`).join("")
            : `<option disabled>No available children</option>`;

        openModal(`
            <h3>Add Parent</h3>
            <input id="parentName" class="search" placeholder="Full Name"><br><br>
            <input id="parentEmail" class="search" placeholder="Email"><br><br>
            <input id="parentPhone" class="search" placeholder="Phone (optional)"><br><br>
            <input id="parentPassword" class="search" type="password" placeholder="Password"><br><br>
            <label>Link Child(ren)</label><br>
            <select id="childSelect" multiple class="search" size="5">${childOptions}</select><br><br>
            <div style="display:flex; justify-content:flex-end; gap:10px">
                <button class="btn" onclick="closeModal()">Cancel</button>
                <button class="btn primary" onclick="saveParent()">Save</button>
            </div>
        `);
    } catch (err) {
        console.error("Error opening Add Parent modal:", err);
        alert("Could not load child dropdowns.");
    }
}

async function saveParent() {
    const selected = Array.from(document.getElementById("childSelect").selectedOptions).map(o => parseInt(o.value));

    const body = {
        fullName: document.getElementById("parentName").value,
        email: document.getElementById("parentEmail").value,
        phone: document.getElementById("parentPhone").value,
        passwordHash: document.getElementById("parentPassword").value,
        status: "Active",
        childIds: selected
    };

    try {
        const res = await fetch("/Parents/Add", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) throw new Error(await res.text());

        tlinkyToast("Parent added successfully");
        closeModal();
        loadParents();
    } catch (err) {
        console.error("Failed to add parent:", err);
        alert("Failed to add parent. Check console for details.");
    }
}

// ---------- Edit ----------
async function openEditParentModal(id) {
    const res = await fetch("/Parents/GetAll");
    const all = await res.json();
    const parent = all.find(p => p.parentId === id);
    if (!parent) return;

    const dropdownRes = await fetch("/Parents/Dropdowns");
    const { children } = await dropdownRes.json();

    const childOptions = children.map(c => `<option value="${c.childId}">${c.fullName}</option>`).join("");

    openModal(`
        <h3>Edit Parent</h3>
        <input id="editParentName" class="search" value="${parent.fullName}"><br><br>
        <input id="editParentEmail" class="search" value="${parent.email}"><br><br>
        <input id="editParentPhone" class="search" value="${parent.phone || ''}"><br><br>
        <input id="editParentPassword" class="search" type="password" placeholder="New Password (optional)"><br><br>
        <label>Reassign Children</label><br>
        <select id="editChildSelect" multiple class="search" size="5">${childOptions}</select><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="updateParent(${id})">Save</button>
        </div>
    `);
}

async function updateParent(id) {
    const selected = Array.from(document.getElementById("editChildSelect").selectedOptions).map(o => parseInt(o.value));

    const body = {
        parentId: id,
        fullName: document.getElementById("editParentName").value,
        email: document.getElementById("editParentEmail").value,
        phone: document.getElementById("editParentPhone").value,
        passwordHash: document.getElementById("editParentPassword").value,
        status: "Active",
        childIds: selected
    };

    try {
        const res = await fetch("/Parents/Edit", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) throw new Error(await res.text());

        tlinkyToast("Parent updated successfully");
        closeModal();
        loadParents();
    } catch (err) {
        console.error("Failed to update parent:", err);
        alert("Failed to update parent. Check console for details.");
    }
}

// ---------- Delete ----------
async function deleteParent(id) {
    if (!confirm("Delete this parent?")) return;
    try {
        const res = await fetch(`/Parents/Delete/${id}`, { method: "DELETE" });
        if (!res.ok) throw new Error(await res.text());
        tlinkyToast("Parent deleted");
        loadParents();
    } catch (err) {
        console.error("Failed to delete parent:", err);
        alert("Error deleting parent.");
    }
}

// ---------- Status update ----------
async function updateParentStatus(id, value) {
    const res = await fetch("/Parents/GetAll");
    const all = await res.json();
    const parent = all.find(p => p.parentId === id);
    if (!parent) return;

    parent.status = value;

    await fetch("/Parents/Edit", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(parent)
    });

    tlinkyToast("Status updated");
}

// ---------- Reset Password ----------
function openResetPasswordModal(id) {
    openModal(`
        <h3>Reset Password</h3>
        <input id="newPassword" type="password" class="search" placeholder="New Password"><br><br>
        <input id="confirmPassword" type="password" class="search" placeholder="Confirm Password"><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="resetPassword(${id})">Save</button>
        </div>
    `);
}

async function resetPassword(id) {
    const pass1 = document.getElementById("newPassword").value;
    const pass2 = document.getElementById("confirmPassword").value;

    if (pass1 !== pass2) {
        alert("Passwords do not match");
        return;
    }

    await fetch(`/Parents/ResetPassword/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(pass1)
    });

    tlinkyToast("Password reset successfully");
    closeModal();
}
