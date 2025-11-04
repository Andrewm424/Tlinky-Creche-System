// ====================================
// 💰 Tlinky Payments Page (Generate + Cloud)
// ====================================

document.addEventListener("DOMContentLoaded", async () => {
    populateMonthDropdown();

    const monthSelect = document.getElementById("monthSelect");
    const btnGenerate = document.getElementById("btnRegenerate");

    btnGenerate.addEventListener("click", async () => {
        const month = monthSelect.value;
        if (!confirm(`Generate payments for ${month}? This will overwrite existing ones.`)) return;

        try {
            btnGenerate.disabled = true;
            btnGenerate.textContent = "Generating...";
            const res = await fetch(`/Payments/Generate/${encodeURIComponent(month)}`, {
                method: "POST"
            });
            if (!res.ok) throw new Error(await res.text());

            const data = await res.json();
            tlinkyToast(`${data.count} payments generated for ${month}`);
            await loadPaymentsByMonth(month);
        } catch (e) {
            console.error("Generate failed:", e);
            alert("Failed to generate payments.");
        } finally {
            btnGenerate.disabled = false;
            btnGenerate.textContent = "Generate";
        }
    });

    await loadPaymentsByMonth(getCurrentMonth());
});

// ---------- Helpers ----------
function getCurrentMonth() {
    return new Date().toLocaleString("default", { month: "long", year: "numeric" });
}

function populateMonthDropdown() {
    const select = document.getElementById("monthSelect");
    const now = new Date();
    for (let i = 0; i < 12; i++) {
        const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
        const label = date.toLocaleString("default", { month: "long", year: "numeric" });
        const opt = document.createElement("option");
        opt.value = label;
        opt.text = label;
        if (i === 0) opt.selected = true;
        select.appendChild(opt);
    }
}

// ---------- Load ----------
async function loadPaymentsByMonth(month) {
    try {
        const encodedMonth = encodeURIComponent(month);
        const res = await fetch(`/Payments/ByMonth/${encodedMonth}`);

        if (!res.ok) {
            console.error("❌ Error loading payments:", await res.text());
            tlinkyToast("Failed to load payments");
            return;
        }

        const data = await res.json();
        const tbody = document.getElementById("paymentsTableBody");
        tbody.innerHTML = "";

        if (!data.length) {
            tbody.innerHTML = `<tr><td colspan="7" class="text-center">No payment records found for ${month}.</td></tr>`;
            return;
        }

        data.forEach(p => {
            tbody.innerHTML += `
            <tr>
                <td>${p.parentName}</td>
                <td>${p.childName}</td>
                <td>${p.month}</td>
                <td>R${Number(p.amount || 0).toFixed(2)}</td>
                <td>
                    <select onchange="updateStatus(${p.paymentId}, this.value)" class="search">
                        <option ${p.status === 'Pending' ? 'selected' : ''}>Pending</option>
                        <option ${p.status === 'Approved' ? 'selected' : ''}>Approved</option>
                        <option ${p.status === 'Rejected' ? 'selected' : ''}>Rejected</option>
                    </select>
                </td>
                <td>
                    ${p.proofUrl
                    ? `<button class="btn primary" onclick="showProofModal('${p.parentName}', '${p.childName}', '${p.month}', '${p.proofUrl}')">View</button>`
                    : '—'
                }
                </td>
                <td>
                    <button class="btn danger" onclick="deletePayment(${p.paymentId})">Delete</button>
                </td>
            </tr>`;
        });
    } catch (err) {
        console.error("⚠️ JS error loading payments:", err);
        tlinkyToast("Unexpected error occurred");
    }
}

// ---------- New: Show Proof Modal ----------
function showProofModal(parentName, childName, month, proofUrl) {
    openModal(`
        <div style="text-align:center">
            <h3>Payment Proof</h3>
            <p><strong>Parent:</strong> ${parentName}</p>
            <p><strong>Child:</strong> ${childName}</p>
            <p><strong>Month:</strong> ${month}</p>
            <img src="${proofUrl}" 
                 alt="Payment Proof" 
                 style="width:100%;max-width:500px;border-radius:10px;margin:15px 0;box-shadow:0 2px 10px rgba(0,0,0,0.15)" />
            <div style="margin-top:10px">
                <button class="btn" onclick="closeModal()">Close</button>
            </div>
        </div>
    `);
}
s

// ---------- Update ----------
async function updateStatus(id, value) {
    await fetch(`/Payments/UpdateStatus/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status: value })
    });
    tlinkyToast("Status updated");
}

// ---------- Delete ----------
async function deletePayment(id) {
    if (!confirm("Delete this payment?")) return;
    await fetch(`/Payments/Delete/${id}`, { method: "DELETE" });
    tlinkyToast("Payment deleted");
    await loadPaymentsByMonth(document.getElementById("monthSelect").value);
}
