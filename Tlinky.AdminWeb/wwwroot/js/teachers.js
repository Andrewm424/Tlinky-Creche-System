// ==================================
// 👩‍🏫 Tlinky Teachers CRUD (fixed)
// ==================================

document.addEventListener("DOMContentLoaded", async () => {
    await loadTeachers();
    document.getElementById("btnAddTeacher")?.addEventListener("click", openAddTeacherModal);
});

// ---------- Load ----------
async function loadTeachers() {
    const tbody = document.getElementById("teachersTableBody");
    tbody.innerHTML = `<tr><td colspan="6">Loading...</td></tr>`;

    try {
        const res = await fetch("/Teachers/GetAll");
        if (!res.ok) throw new Error("Failed to fetch teachers");
        const data = await res.json();

        tbody.innerHTML = "";
        if (!data.length) {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center">No teachers found.</td></tr>`;
            return;
        }

        data.forEach(t => {
            tbody.innerHTML += `
                <tr>
                    <td><img src="${t.photoUrl || '/images/default.png'}"
                             style="width:40px;height:40px;border-radius:50%"></td>
                    <td>${t.fullName}</td>
                    <td>${t.email}</td>
                    <td>${t.classCount || 0}</td>
                    <td>
                        <select class="search" onchange="updateStatus(${t.teacherId}, this.value)">
                            <option ${t.status === "Active" ? "selected" : ""}>Active</option>
                            <option ${t.status === "Suspended" ? "selected" : ""}>Suspended</option>
                        </select>
                    </td>
                    <td>
                        <button class="btn" onclick="openEditTeacherModal(${t.teacherId})">Edit</button>
                        <button class="btn" onclick="openResetPasswordModal(${t.teacherId})">Reset Password</button>
                        <button class="btn danger" onclick="deleteTeacher(${t.teacherId})">Delete</button>
                    </td>
                </tr>`;
        });
    } catch (err) {
        console.error("Error loading teachers:", err);
        tbody.innerHTML = `<tr><td colspan="6" class="text-danger text-center">Error loading teachers</td></tr>`;
    }
}

// ---------- Add ----------
function openAddTeacherModal() {
    openModal(`
        <h3>Add Teacher</h3>
        <input id="teacherFullName" class="search" placeholder="Full Name"><br><br>
        <input id="teacherEmail" class="search" placeholder="Email"><br><br>
        <input id="teacherPassword" class="search" type="password" placeholder="Password"><br><br>
        <label>Photo</label><br>
        <input id="teacherPhoto" class="search" type="file" accept="image/*"><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="saveTeacher()">Save</button>
        </div>
    `);
}

async function saveTeacher() {
    try {
        const fullName = document.getElementById("teacherFullName").value.trim();
        const email = document.getElementById("teacherEmail").value.trim();
        const password = document.getElementById("teacherPassword").value.trim();
        const file = document.getElementById("teacherPhoto").files[0];

        if (!fullName || !email || !password) {
            alert("Please fill in all fields.");
            return;
        }

        let photoUrl = null;
        if (file) {
            const fd = new FormData();
            fd.append("file", file);
            const upload = await fetch("/api/upload/teacher-photo", { method: "POST", body: fd });
            const uploaded = await upload.json();
            photoUrl = uploaded.url;
        }

        const body = {
            fullName,
            email,
            passwordHash: password,
            photoUrl,
            status: "Active"
        };

        const res = await fetch("/Teachers/Add", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) throw new Error(await res.text());

        tlinkyToast("Teacher added successfully");
        closeModal();
        loadTeachers();
    } catch (e) {
        console.error(e);
        alert("Error adding teacher.");
    }
}

// ---------- Edit ----------
async function openEditTeacherModal(id) {
    const res = await fetch("/Teachers/GetAll");
    const all = await res.json();
    const t = all.find(x => x.teacherId === id);
    if (!t) return;

    openModal(`
        <h3>Edit Teacher</h3>
        <img src="${t.photoUrl || '/images/default.png'}"
             style="width:60px;height:60px;border-radius:50%;margin-bottom:10px;"><br>
        <input id="editTeacherFullName" class="search" value="${t.fullName}"><br><br>
        <input id="editTeacherEmail" class="search" value="${t.email}"><br><br>
        <input id="editTeacherPassword" class="search" type="password" placeholder="New Password (optional)"><br><br>
        <label>Replace Photo</label><br>
        <input id="editTeacherPhoto" class="search" type="file" accept="image/*"><br><br>
        <div style="display:flex; justify-content:flex-end; gap:10px">
            <button class="btn" onclick="closeModal()">Cancel</button>
            <button class="btn primary" onclick="updateTeacher(${id})">Save</button>
        </div>
    `);
}

async function updateTeacher(id) {
    let photoUrl = null;
    const file = document.getElementById("editTeacherPhoto").files[0];
    const newPassword = document.getElementById("editTeacherPassword").value.trim();

    if (file) {
        const fd = new FormData();
        fd.append("file", file);
        const upload = await fetch("/api/upload/teacher-photo", { method: "POST", body: fd });
        const uploaded = await upload.json();
        photoUrl = uploaded.url;
    }

    const body = {
        teacherId: id,
        fullName: document.getElementById("editTeacherFullName").value.trim(),
        email: document.getElementById("editTeacherEmail").value.trim(),
        passwordHash: newPassword,
        photoUrl,
        status: "Active"
    };

    const res = await fetch("/Teachers/Edit", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });

    if (!res.ok) throw new Error(await res.text());

    tlinkyToast("Teacher updated successfully");
    closeModal();
    loadTeachers();
}

// ---------- Delete ----------
async function deleteTeacher(id) {
    if (!confirm("Delete this teacher?")) return;
    await fetch(`/Teachers/Delete/${id}`, { method: "DELETE" });
    tlinkyToast("Teacher deleted");
    loadTeachers();
}

// ---------- Status ----------
async function updateStatus(id, value) {
    const res = await fetch("/Teachers/GetAll");
    const all = await res.json();
    const t = all.find(x => x.teacherId === id);
    if (!t) return;
    t.status = value;

    await fetch("/Teachers/Edit", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(t)
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
            <button class="btn primary" onclick="resetTeacherPassword(${id})">Save</button>
        </div>
    `);
}

async function resetTeacherPassword(id) {
    const pass1 = document.getElementById("newPassword").value;
    const pass2 = document.getElementById("confirmPassword").value;

    if (pass1 !== pass2) {
        alert("Passwords do not match");
        return;
    }

    await fetch(`/Teachers/ResetPassword/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(pass1)
    });

    tlinkyToast("Password reset successfully");
    closeModal();
}
