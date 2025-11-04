document.addEventListener("DOMContentLoaded", async () => {
    await loadAnnouncements();

    document.getElementById("btnAddAnnouncement").addEventListener("click", () => {
        openModal(`
          <h3>New Announcement</h3>
          <input id='annTitle' class='search' placeholder='Title' /><br><br>
          <textarea id='annMsg' class='search' placeholder='Message' style='width:100%; height:100px'></textarea><br><br>

          <label>Audience</label>
          <select id='annAudience' class='search' style='width:100%; margin-top:6px'>
            <option>Everyone</option>
            <option>Teachers</option>
            <option>Parents</option>
          </select><br><br>

          <input id='annAuthor' class='search' placeholder='Posted by (e.g. Mrs Dlamini)' /><br><br>

          <div style='display:flex; justify-content:flex-end; gap:10px'>
            <button type='button' class='btn' onclick='closeModal()'>Cancel</button>
            <button type='button' class='btn primary' onclick='saveAnnouncement()'>Post</button>
          </div>
        `);
    });
});


// ✅ Load all announcements
async function loadAnnouncements() {
    const list = document.getElementById("announcementList");
    list.innerHTML = `<li class='card'><p>Loading...</p></li>`;

    try {
        const res = await fetch("/Announcements/GetAll");
        if (!res.ok) throw new Error(await res.text());

        const data = await res.json();
        list.innerHTML = "";

        if (!data.length) {
            list.innerHTML = `<li class='card'><p>No announcements yet.</p></li>`;
            return;
        }

        data.forEach(a => {
            list.innerHTML += `
              <li class="card">
                  <div style="display:flex; justify-content:space-between; align-items:center">
                      <h4 style="margin:0">${a.title}</h4>
                      <span class="badge">${a.audience}</span>
                  </div>
                  <p>${a.message}</p>
                  <small>📅 ${new Date(a.datePosted).toLocaleString()}${a.author ? " • ✏️ " + a.author : ""}</small>
                  <div style="margin-top:8px">
                      <button class="btn danger" onclick="deleteAnnouncement(${a.announcementId})">Delete</button>
                  </div>
              </li>`;
        });
    } catch (err) {
        console.error("Error loading announcements:", err);
        list.innerHTML = `<li class='card'><p>⚠️ Failed to load announcements.</p></li>`;
    }
}


// ✅ Save a new announcement
async function saveAnnouncement() {
    const title = document.getElementById("annTitle").value.trim();
    const message = document.getElementById("annMsg").value.trim();
    const audience = document.getElementById("annAudience").value;
    const author = document.getElementById("annAuthor").value.trim();

    if (!title || !message) {
        alert("Please provide both title and message.");
        return;
    }

    const body = { title, message, audience, author };

    try {
        const res = await fetch("/Announcements/Add", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) throw new Error(await res.text());

        tlinkyToast("✅ Announcement posted!");
        closeModal();
        await loadAnnouncements();
    } catch (err) {
        console.error("Save announcement failed:", err);
        alert("❌ Failed to save announcement");
    }
}


// ✅ Delete announcement
async function deleteAnnouncement(id) {
    if (!confirm("Are you sure you want to delete this announcement?")) return;

    try {
        const res = await fetch(`/Announcements/Delete/${id}`, { method: "DELETE" });

        if (!res.ok) throw new Error(await res.text());

        tlinkyToast("🗑️ Announcement deleted");
        await loadAnnouncements();
    } catch (err) {
        console.error("Delete failed:", err);
        alert("❌ Failed to delete announcement");
    }
}
