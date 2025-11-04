async function editSchool() {
    const res = await fetch('/Settings/Index');
    const data = await res.text();
    openModal(`
      <h3>Edit School Info</h3>
      <input id='sName' class='search' placeholder='School Name'><br><br>
      <input id='sEmail' class='search' placeholder='Email'><br><br>
      <input id='sPhone' class='search' placeholder='Phone'><br><br>
      <input id='sPrincipal' class='search' placeholder='Principal'><br><br>
      <div style='text-align:right'>
        <button class='btn' onclick='closeModal()'>Cancel</button>
        <button class='btn primary' onclick='saveSchool()'>Save</button>
      </div>
    `);
}

async function saveSchool() {
    const body = {
        schoolName: document.getElementById('sName').value,
        email: document.getElementById('sEmail').value,
        phone: document.getElementById('sPhone').value,
        principal: document.getElementById('sPrincipal').value
    };
    const res = await fetch('/Settings/UpdateSchool', {
        method: 'PUT', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    if (res.ok) { tlinkyToast('School info updated'); location.reload(); }
    else alert('Failed to update');
}

async function editFees() {
    openModal(`
      <h3>Edit Fees</h3>
      <input id='base' class='search' placeholder='Base Monthly Fee'><br><br>
      <input id='toddler' class='search' placeholder='Toddler Fee'><br><br>
      <input id='preschool' class='search' placeholder='Preschool Fee'><br><br>
      <input id='late' class='search' placeholder='Late Fee'><br><br>
      <label><input type='checkbox' id='latePolicy'> Enable Late Policy</label><br><br>
      <div style='text-align:right'>
        <button class='btn' onclick='closeModal()'>Cancel</button>
        <button class='btn primary' onclick='saveFees()'>Save</button>
      </div>
    `);
}

async function saveFees() {
    const body = {
        baseMonthlyFee: parseFloat(document.getElementById('base').value),
        toddlerFee: parseFloat(document.getElementById('toddler').value),
        preschoolFee: parseFloat(document.getElementById('preschool').value),
        lateFeeAmount: parseFloat(document.getElementById('late').value),
        lateFeePolicy: document.getElementById('latePolicy').checked
    };
    const res = await fetch('/Settings/UpdateFees', {
        method: 'PUT', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    if (res.ok) { tlinkyToast('Fees updated'); location.reload(); }
    else alert('Failed to update fees');
}

async function editPrefs() {
    openModal(`
      <h3>Edit Preferences</h3>
      <label><input type='checkbox' id='notif'> Enable Notifications</label><br><br>
      <input id='term' class='search' placeholder='Term Dates'><br><br>
      <div style='text-align:right'>
        <button class='btn' onclick='closeModal()'>Cancel</button>
        <button class='btn primary' onclick='savePrefs()'>Save</button>
      </div>
    `);
}

async function savePrefs() {
    const body = {
        notificationsEnabled: document.getElementById('notif').checked,
        termDates: document.getElementById('term').value
    };
    const res = await fetch('/Settings/UpdatePrefs', {
        method: 'PUT', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    if (res.ok) { tlinkyToast('Preferences updated'); location.reload(); }
    else alert('Failed to update preferences');
}
