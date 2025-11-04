// ======================================
// 🌼 Tlinky Children CRUD (Final Stable)
// ======================================

document.addEventListener("DOMContentLoaded", async () => {
    await loadChildren();

    const btn = document.getElementById("btnAddChild");
    if (btn) {
        btn.addEventListener("click", async () => {
            const { classes } = await (await fetch("/Children/Dropdowns")).json();
            openAddChildModal(classes);
        });
    }
});

// ---------- Load all ----------
async function loadChildren() {
    try {
        const res = await fetch("/Children/GetAll");
        const data = await res.json();
        const tbody = document.getElementById("childrenTableBody");
        tbody.innerHTML = "";

        if (!data.length) {
            tbody.innerHTML = `<tr><td colspan="7" class="text-center">No children found.</td></tr>`;
            return;
        }

        data.forEach(c => {
            tbody.innerHTML += `
            <tr>
                <td><img src="${c.photoUrl || '/images/default.png'}"
                         style="width:40px;height:40px;border-radius:50%"></td>
                <td>${c.fullName}</td>
                <td>${c.dob || '-'}</td>
                <td>${c.className}</td>
                <td>${c.allergies || '-'}</td>
                <td>
                    <select class="search" onchange="updateStatus(${c.childId}, this.value)">
                        <option ${c.status === 'Active' ? 'selected' : ''}>Active</option>
                        <option ${c.status === 'Suspended' ? 'selected' : ''}>Suspended</option>
                    </select>
                </td>
                <td>
                    <button class="btn" onclick="openEditModal(${c.childId})">Edit</button>
                    <button class="btn danger" onclick="deleteChild(${c.childId})">Delete</button>
                </td>
            </tr>`;
        });
    } catch (err) {
        console.error("⚠️ Error loading children:", err);
        tlinkyToast("⚠️ Could not load children. Please check your connection.");
    }
}

// ---------- Add Modal ----------
async function openAddChildModal(classes) {
    const clsOptions = classes.map(c => `<option value="${c.classId}">${c.name}</option>`).join('');

    openModal(`
        <h3>Add Child</h3>
        <input id="childName" class="search" placeholder="Full Name" required><br><br>
        <label>DOB</label>
        <input id="childDob" class="search" type="date"><br><br>
        <label>Class</label>
        <select id="childClass" class="search"><option value="">Select</option>${clsOptions}</select><br><br>
        <input id="childAllergies" class="search" placeholder="Allergies (optional)"><br><br>
        <label>Photo</label>
        <input id="childPhoto" class="search" type="file" accept="image/*"><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="saveChild()">Save</button>
        </div>
    `);
}

// ---------- Save ----------
// ---------- Save ----------
async function saveChild() {
    try {
        // ✅ Capture values early before anything triggers a re-render
        const nameValue = document.getElementById("childName")?.value.trim() || "";
        const dobValue = document.getElementById("childDob")?.value || null;
        const classValue = parseInt(document.getElementById("childClass")?.value) || null;
        const allergyValue = document.getElementById("childAllergies")?.value.trim() || null;
        const fileInput = document.getElementById("childPhoto");
        const photoFile = fileInput ? fileInput.files[0] : null;

        if (!nameValue) {
            tlinkyToast("⚠️ Please enter the child's full name.");
            return;
        }

        // ✅ Upload photo first (if exists)
        let photoUrl = null;
        if (photoFile) {
            const fd = new FormData();
            fd.append("file", photoFile);

            const upload = await fetch("/api/upload/child-photo", { method: "POST", body: fd });
            if (!upload.ok) throw new Error("Photo upload failed");
            const uploaded = await upload.json();
            photoUrl = uploaded.url;
        }

        // ✅ Build payload AFTER upload to avoid losing field data
        const body = {
            fullName: nameValue,
            dob: dobValue,
            classId: classValue,
            allergies: allergyValue,
            photoUrl: photoUrl,
            status: "Active"
        };

        const res = await fetch("/Children/Add", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) {
            const msg = await res.text();
            console.error("❌ Failed to add child:", msg);
            tlinkyToast("❌ Could not add child. Please check your inputs.");
            return;
        }

        closeModal();
        tlinkyToast("✅ Child added successfully!");
        await loadChildren();
    } catch (err) {
        console.error("⚠️ Error adding child:", err);
        tlinkyToast("⚠️ Unexpected error while adding child. Check console for details.");
    }
}


// ---------- Edit ----------
async function openEditModal(id) {
    const res = await fetch("/Children/GetAll");
    const all = await res.json();
    const child = all.find(c => c.childId === id);
    if (!child) return;

    const { classes } = await (await fetch("/Children/Dropdowns")).json();
    const clsOpts = classes.map(c =>
        `<option value="${c.classId}" ${c.name === child.className ? 'selected' : ''}>${c.name}</option>`
    ).join('');

    openModal(`
        <h3>Edit Child</h3>
        <img src="${child.photoUrl || '/images/default.png'}" 
             style="width:60px;height:60px;border-radius:50%;margin-bottom:10px;"><br>
        <input id="editName" class="search" value="${child.fullName}"><br><br>
        <label>DOB</label>
        <input id="editDob" class="search" type="date" value="${child.dob}"><br><br>
        <label>Class</label>
        <select id="editClass" class="search">${clsOpts}</select><br><br>
        <input id="editAllergies" class="search" value="${child.allergies || ''}"><br><br>
        <label>Replace Photo</label>
        <input id="editPhoto" class="search" type="file" accept="image/*"><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="updateChild(${id})">Save</button>
        </div>
    `);
}

// ---------- Update ----------
async function updateChild(id) {
    try {
        const name = document.getElementById("editName").value.trim();
        const dob = document.getElementById("editDob").value || null;
        const classId = parseInt(document.getElementById("editClass").value) || null;
        const allergies = document.getElementById("editAllergies").value.trim() || null;
        const file = document.getElementById("editPhoto").files[0];

        let photoUrl = null;
        if (file) {
            const fd = new FormData();
            fd.append("file", file);
            const upload = await fetch("/api/upload/child-photo", { method: "POST", body: fd });
            if (!upload.ok) throw new Error("Photo upload failed.");
            const uploaded = await upload.json();
            photoUrl = uploaded.url;
        }

        const body = { childId: id, fullName: name, dob, classId, allergies, photoUrl, status: "Active" };

        const res = await fetch("/Children/Edit", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) {
            const msg = await res.text();
            console.error("❌ Failed to update child:", msg);
            tlinkyToast("❌ Failed to update child. Please check inputs.");
            return;
        }

        closeModal();
        tlinkyToast("✅ Child updated successfully!");
        await loadChildren();
    } catch (err) {
        console.error("⚠️ Error updating child:", err);
        tlinkyToast("⚠️ Unexpected error while updating child.");
    }
}

// ---------- Delete ----------
window.deleteChild = async function (id) {
    if (!confirm("Are you sure you want to delete this child?")) return;

    try {
        const res = await fetch(`/Children/Delete?id=${id}`, { method: "DELETE" });
        const result = await res.json();

        if (!res.ok) {
            console.error("❌ Delete failed:", result);
            tlinkyToast(result.message || "❌ Failed to delete child. It may have linked records.");
            return;
        }

        tlinkyToast(result.message || "✅ Child deleted successfully!");
        await loadChildren();
    } catch (err) {
        console.error("⚠️ Error deleting child:", err);
        tlinkyToast("⚠️ Error deleting child. Check console for details.");
    }
};

// ---------- Status update ----------
async function updateStatus(id, value) {
    try {
        const res = await fetch("/Children/GetAll");
        const all = await res.json();
        const child = all.find(c => c.childId === id);
        if (!child) return;
        child.status = value;

        await fetch("/Children/Edit", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(child)
        });
        tlinkyToast("Status updated");
    } catch (err) {
        console.error("⚠️ Error updating status:", err);
        tlinkyToast("⚠️ Failed to update status.");
    }
}
