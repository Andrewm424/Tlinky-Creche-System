// =====================================
// 🌼 Tlinky Classes CRUD (Live Neon DB)
// =====================================

document.addEventListener("DOMContentLoaded", async () => {
    await loadClasses();

    document.getElementById("btnAddClass").addEventListener("click", async () => {
        const { teachers } = await (await fetch("/Classes/Dropdowns")).json();
        openAddModal(teachers);
    });
});

// ---------- Load classes ----------
async function loadClasses() {
    const res = await fetch("/Classes/GetAll");
    const data = await res.json();
    const tbody = document.getElementById("classesTableBody");
    tbody.innerHTML = "";

    if (!data.length) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center">No classes found.</td></tr>`;
        return;
    }

    data.forEach(c => {
        tbody.innerHTML += `
        <tr>
            <td>${c.name}</td>
            <td>${c.studentCount}</td>
            <td>${c.teacherName}</td>
            <td>
                <select class="search" onchange="updateStatus(${c.classId}, this.value)">
                    <option ${c.status === 'Active' ? 'selected' : ''}>Active</option>
                    <option ${c.status === 'Inactive' ? 'selected' : ''}>Inactive</option>
                </select>
            </td>
            <td>
                <button class="btn" onclick="openEditModal(${c.classId})">Edit</button>
                <button class="btn danger" onclick="deleteClass(${c.classId})">Delete</button>
            </td>
        </tr>`;
    });
}

// ---------- Add ----------
async function openAddModal(teachers) {
    const teacherOptions = teachers.map(t => `<option value="${t.teacherId}">${t.fullName}</option>`).join('');

    openModal(`
        <h3>Add Class</h3>
        <input id="className" class="search" placeholder="Class Name"><br><br>
        <label>Assign Teacher</label>
        <select id="classTeacher" class="search">
            <option value="">Select Teacher</option>
            ${teacherOptions}
        </select><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="saveClass()">Save</button>
        </div>
    `);
}

async function saveClass() {
    const body = {
        name: document.getElementById("className").value,
        teacherId: parseInt(document.getElementById("classTeacher").value) || null,
        status: "Active"
    };

    const res = await fetch("/Classes/Add", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });

    if (res.ok) {
        tlinkyToast("✅ Class added");
        closeModal();
        loadClasses();
    } else {
        tlinkyToast("⚠️ Failed to add class");
    }
}

// ---------- Edit ----------
async function openEditModal(id) {
    const res = await fetch("/Classes/GetAll");
    const all = await res.json();
    const cls = all.find(c => c.classId === id);
    if (!cls) return;

    const { teachers } = await (await fetch("/Classes/Dropdowns")).json();
    const teacherOptions = teachers.map(t =>
        `<option value="${t.teacherId}" ${t.fullName === cls.teacherName ? 'selected' : ''}>${t.fullName}</option>`
    ).join('');

    openModal(`
        <h3>Edit Class</h3>
        <input id="editName" class="search" value="${cls.name}"><br><br>
        <label>Assign Teacher</label>
        <select id="editTeacher" class="search">${teacherOptions}</select><br><br>
        <label>Status</label>
        <select id="editStatus" class="search">
            <option ${cls.status === 'Active' ? 'selected' : ''}>Active</option>
            <option ${cls.status === 'Inactive' ? 'selected' : ''}>Inactive</option>
        </select><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="updateClass(${id})">Save</button>
        </div>
    `);
}

async function updateClass(id) {
    const body = {
        classId: id,
        name: document.getElementById("editName").value,
        teacherId: parseInt(document.getElementById("editTeacher").value) || null,
        status: document.getElementById("editStatus").value
    };

    const res = await fetch("/Classes/Edit", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });

    if (res.ok) {
        tlinkyToast("✅ Class updated");
        closeModal();
        loadClasses();
    } else {
        tlinkyToast("⚠️ Update failed");
    }
}

// ---------- Delete ----------
async function deleteClass(id) {
    if (!confirm("Are you sure you want to delete this class?")) return;

    try {
        const res = await fetch(`/Classes/Delete?id=${id}`, { method: "DELETE" });
        if (!res.ok) throw new Error("Delete failed");

        const result = await res.json();
        tlinkyToast(result.message || "✅ Class deleted");
        await loadClasses();
    } catch (err) {
        console.error(err);
        tlinkyToast("⚠️ Could not delete class");
    }
}

// ---------- Update Status ----------
async function updateStatus(id, value) {
    const res = await fetch("/Classes/GetAll");
    const all = await res.json();
    const cls = all.find(c => c.classId === id);
    if (!cls) return;
    cls.status = value;

    await fetch("/Classes/Edit", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(cls)
    });
    tlinkyToast("🔄 Status updated");
}
