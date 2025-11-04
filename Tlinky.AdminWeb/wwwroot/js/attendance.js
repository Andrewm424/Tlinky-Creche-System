// =====================================
// 🕒 Tlinky Attendance Management (Admin)
// =====================================

const LS_CLASS_KEY = "tlinky_attendance_classId";
const LS_DATE_KEY = "tlinky_attendance_date";

// Small helper: read JSON safely
function safeParse(text) {
    try { return text ? JSON.parse(text) : null; } catch { return null; }
}

document.addEventListener("DOMContentLoaded", async () => {
    const dateInput = document.getElementById("attendanceDate");
    const classSelect = document.getElementById("classSelect");

    // Restore last used date/class (or default to today)
    const storedDate = localStorage.getItem(LS_DATE_KEY);
    dateInput.value = storedDate || new Date().toISOString().split("T")[0];

    await loadClasses(); // this will set options and load children

    // Restore last class if available
    const storedClassId = localStorage.getItem(LS_CLASS_KEY);
    if (storedClassId && [...classSelect.options].some(o => o.value == storedClassId)) {
        classSelect.value = storedClassId;
        await loadChildren(storedClassId);
    }

    // Always overlay DB state for current selection
    if (classSelect.value) {
        await loadExistingAttendance(classSelect.value);
    }

    // Handlers
    document.getElementById("btnMarkAll").addEventListener("click", markAllPresent);
    document.getElementById("btnSaveAttendance").addEventListener("click", saveAttendance);

    classSelect.addEventListener("change", async (e) => {
        localStorage.setItem(LS_CLASS_KEY, e.target.value);
        await loadChildren(e.target.value);
        await loadExistingAttendance(e.target.value);
    });

    dateInput.addEventListener("change", async () => {
        localStorage.setItem(LS_DATE_KEY, dateInput.value);
        const cid = classSelect.value;
        if (cid) await loadExistingAttendance(cid);
    });
});

// ---------- Load Classes ----------
async function loadClasses() {
    const select = document.getElementById("classSelect");
    const tbody = document.getElementById("attendanceTableBody");

    tbody.innerHTML = "<tr><td colspan='3'>Loading classes...</td></tr>";

    try {
        const res = await fetch("/Classes/GetAll");
        if (!res.ok) throw new Error("Failed to load classes");
        const data = await res.json();

        select.innerHTML = data.map(c => `<option value="${c.classId}">${c.name}</option>`).join("");

        if (data.length > 0) {
            // Do not force the first class here — we restore it from localStorage in DOMContentLoaded
            tbody.innerHTML = "<tr><td colspan='3'>Select a class to load children…</td></tr>";
        } else {
            tbody.innerHTML = "<tr><td colspan='3'>No classes found</td></tr>";
        }
    } catch (err) {
        console.error(err);
        select.innerHTML = "<option>Error loading classes</option>";
        tbody.innerHTML = "<tr><td colspan='3'>Error loading classes</td></tr>";
    }
}

// ---------- Load Children by Class ----------
async function loadChildren(classId) {
    const tbody = document.getElementById("attendanceTableBody");
    tbody.innerHTML = "<tr><td colspan='3'>Loading children...</td></tr>";

    try {
        const res = await fetch(`/Children/GetByClass/${classId}`);
        if (!res.ok) throw new Error("Failed to load children");
        const children = await res.json();

        if (!children.length) {
            tbody.innerHTML = "<tr><td colspan='3'>No children found</td></tr>";
            return;
        }

        // Rebuild rows fresh
        tbody.innerHTML = "";
        children.forEach(c => {
            tbody.innerHTML += `
                <tr data-child="${c.childId}">
                    <td>${c.fullName}</td>
                    <td>
                        <select class="search status-select">
                            <option value="Present">Present</option>
                            <option value="Absent">Absent</option>
                            <option value="Late">Late</option>
                        </select>
                    </td>
                    <td><input class="search note-input" placeholder="Add note…"></td>
                </tr>`;
        });
    } catch (err) {
        console.error(err);
        tbody.innerHTML = "<tr><td colspan='3'>Error loading children</td></tr>";
    }
}

// ---------- Load Existing Attendance (overlays DB state) ----------
async function loadExistingAttendance(classId) {
    const date = document.getElementById("attendanceDate").value;
    const tbody = document.getElementById("attendanceTableBody");
    if (!classId || !date) return;

    try {
        const res = await fetch(`/api/AttendanceApi?date=${encodeURIComponent(date)}&classId=${encodeURIComponent(classId)}`);
        if (!res.ok) {
            console.warn("Attendance GET not OK:", res.status);
            return;
        }
        const text = await res.text();
        const data = safeParse(text);
        if (!Array.isArray(data) || data.length === 0) {
            // Nothing saved yet for this class+date; leave defaults as-is
            return;
        }

        // Reset row background (in case of previous highlights)
        document.querySelectorAll("tbody tr").forEach(row => { row.style.background = "transparent"; });

        // Apply existing attendance from DB
        data.forEach(a => {
            const row = document.querySelector(`tr[data-child="${a.childId}"]`);
            if (!row) return;
            row.querySelector(".status-select").value = a.status;
            row.querySelector(".note-input").value = a.notes || "";
            // soft tint for persisted rows
            row.style.background = "#f9fff9";
        });
    } catch (err) {
        console.error("Error loading attendance:", err);
        tbody.innerHTML = "<tr><td colspan='3'>Error loading attendance</td></tr>";
    }
}

// ---------- Mark All Present ----------
function markAllPresent() {
    document.querySelectorAll(".status-select").forEach(sel => (sel.value = "Present"));
    tlinkyToast("All marked Present");
}

// ---------- Save Attendance ----------
async function saveAttendance() {
    const date = document.getElementById("attendanceDate").value;
    const classId = document.getElementById("classSelect").value;
    const rows = document.querySelectorAll("tbody tr");

    if (!classId) {
        alert("Please select a class first.");
        return;
    }

    const records = Array.from(rows).map(row => ({
        childId: parseInt(row.dataset.child),
        teacherId: null,
        date: new Date(date).toISOString(), // stored UTC midnight for that date
        status: row.querySelector(".status-select").value,
        notes: row.querySelector(".note-input").value || ""
    }));

    try {
        const res = await fetch("/api/AttendanceApi", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(records)
        });

        const text = await res.text();
        const data = safeParse(text);

        if (res.ok && data && data.success) {
            tlinkyToast(data.message || "Attendance saved successfully");

            // Persist current selection so a full page refresh restores the same slice
            localStorage.setItem(LS_CLASS_KEY, classId);
            localStorage.setItem(LS_DATE_KEY, date);

            // Reload latest DB snapshot for the same class/date (ensures UI matches DB)
            await loadExistingAttendance(classId);

            // Highlight updated rows briefly
            document.querySelectorAll("tbody tr").forEach(row => {
                row.style.transition = "background 0.3s";
                row.style.background = "#e6ffe6";
                setTimeout(() => (row.style.background = "#f9fff9"), 700); // settle into persisted tint
            });
        } else {
            console.error("Save failed:", data || text);
            alert("Failed to save attendance. Check console for details.");
        }
    } catch (err) {
        console.error("Error saving attendance:", err);
        alert("Unexpected error occurred while saving attendance.");
    }
}
