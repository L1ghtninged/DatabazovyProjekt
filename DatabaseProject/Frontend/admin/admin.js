function debugLog(message, data = null) {
    console.log(`[Admin JS] ${message}`, data || '');
}

const loginForm = document.getElementById("loginForm");
if (loginForm) {
    loginForm.addEventListener("submit", async e => {
        e.preventDefault();

        const email = document.getElementById("email").value;
        debugLog("Pokus o přihlášení", { email });

        try {
            const admin = await apiRequest("/admin/login", "POST", { email });
            debugLog("Přihlášení úspěšné", admin);

            localStorage.setItem("admin", JSON.stringify(admin));
            debugLog("Admin uložen do localStorage");

            window.location.href = "dashboard.html";
        } catch (err) {
            debugLog("Chyba přihlášení", err.message);
            document.getElementById("error").innerText = "Chybný formát";
        }
    });
}

const registerForm = document.getElementById("registerForm");
if (registerForm) {
    registerForm.addEventListener("submit", async e => {
        e.preventDefault();

        const firstName = document.getElementById("firstName").value;
        const lastName = document.getElementById("lastName").value;
        const email = document.getElementById("email").value;
        
        debugLog("Pokus o registraci", { firstName, lastName, email });

        try {
            const result = await apiRequest("/admin/register", "POST", {
                firstName: firstName,
                lastName: lastName,
                email: email
            });
            
            debugLog("Registrace úspěšná", result);
            
            document.getElementById("success").innerHTML = `
                <p>Registrace úspěšná!</p>
                <p>ID: ${result.id}</p>
                <p>Jméno: ${result.fullName}</p>
                <p>Email: ${result.email}</p>
                <button onclick="loginAfterRegister('${email}')" style="margin-top: 10px;">
                    Přihlásit se
                </button>
            `;
            
            registerForm.reset();
            
        } catch (err) {
            debugLog("Chyba registrace", err.message);
            document.getElementById("error").innerText = "Chybný formát";
        }
    });
}

window.loginAfterRegister = async function(email) {
    debugLog("Automatické přihlášení po registraci", { email });
    
    try {
        const admin = await apiRequest("/admin/login", "POST", { email });
        debugLog("Auto-login úspěšný", admin);
        
        localStorage.setItem("admin", JSON.stringify(admin));
        window.location.href = "dashboard.html";
        
    } catch (err) {
        debugLog("Chyba auto-loginu", err.message);
        alert("Registrace byla úspěšná, ale přihlášení selhalo: Chybný formát");
    }
}

document.addEventListener("DOMContentLoaded", function() {
    debugLog("DOMContentLoaded - kontrola přihlášení");
    
    const adminInfo = document.getElementById("adminInfo");
    if (adminInfo) {
        const adminData = localStorage.getItem("admin");
        debugLog("Data v localStorage", adminData);
        
        if (!adminData) {
            debugLog("Žádný admin v localStorage - přesměrování na login");
            window.location.href = "index.html";
            return;
        }
        
        try {
            const admin = JSON.parse(adminData);
            debugLog("Admin z localStorage", admin);
            
            adminInfo.innerHTML = `
                <strong>Přihlášen:</strong> ${admin.firstName} ${admin.lastName} (${admin.email})<br>
                <small>ID: ${admin.id}</small>
            `;
            
            setTimeout(() => loadAdminStatistics(), 100);
            setTimeout(() => loadRequests(), 200);
            setTimeout(() => loadAdmins(), 300);
            
        } catch (e) {
            debugLog("Chyba parsování admina", e.message);
            localStorage.removeItem("admin");
            window.location.href = "index.html";
        }
    }
});

function getLoggedAdmin() {
    const adminData = localStorage.getItem("admin");
    if (!adminData) {
        debugLog("getLoggedAdmin: žádný admin - přesměrování");
        window.location.href = "index.html";
        return null;
    }
    
    try {
        return JSON.parse(adminData);
    } catch (e) {
        debugLog("getLoggedAdmin: chyba parsování", e.message);
        localStorage.removeItem("admin");
        window.location.href = "index.html";
        return null;
    }
}

async function loadRequests() {
    try {
        const container = document.getElementById("requests");
        if (!container) return;
        
        container.innerHTML = "<p>Načítám požadavky...</p>";
        
        debugLog("Načítání requests overview");
        const requests = await apiRequest("/requests/overview", "GET");
        debugLog("Requests načteny", requests ? requests.length : 0);
        
        container.innerHTML = `
            <div class="filters">
                <button onclick="loadRequests()" class="filter-btn active">Všechny</button>
                <button onclick="loadRequestsByStatus('novy', event)" class="filter-btn">Nové</button>
                <button onclick="loadRequestsByStatus('resise', event)" class="filter-btn">Přiřazené</button>
                <button onclick="loadRequestsByStatus('uzavreny', event)" class="filter-btn">Dokončené</button>
                <button onclick="loadRequestsByStatus('storno', event)" class="filter-btn">Zrušené</button>
            </div>
            <div id="requestsList"></div>
        `;
        
        displayRequests(requests);
        
    } catch (err) {
        debugLog("Chyba při načítání požadavků", err.message);
        const container = document.getElementById("requests");
        if (container) {
            container.innerHTML = `<p class="error">Chybný formát</p>`;
        }
    }
}

async function loadRequestsByStatus(status, event = null) {
    try {
        debugLog(`Načítání requests se statusem: ${status}`);
        const requests = await apiRequest(`/requests/overview/${status}`, "GET");
        
        if (event && event.target) {
            document.querySelectorAll('.filter-btn').forEach(btn => {
                btn.classList.remove('active');
            });
            event.target.classList.add('active');
        }
        
        displayRequests(requests);
    } catch (err) {
        debugLog("Chyba při načítání požadavků podle statusu", err.message);
        alert("Chybný formát");
    }
}

function displayRequests(requests) {
    const requestsList = document.getElementById("requestsList");
    
    if (!requestsList) return;
    
    requestsList.innerHTML = "";
    
    if (!requests || requests.length === 0) {
        requestsList.innerHTML = "<p>Žádné požadavky k zobrazení</p>";
        return;
    }
    
    debugLog(`Zobrazuji ${requests.length} requestů`);
    
    requests.forEach(req => {
        const reqDiv = document.createElement("div");
        reqDiv.className = "request-item";
        reqDiv.innerHTML = `
            <div class="request-header">
                <div>
                    <h3>Požadavek #${req.requestId}</h3>
                    <p class="request-meta">
                        <strong>Klient:</strong> ${req.contactFirstName} ${req.contactLastName} | 
                        <strong>Email:</strong> ${req.contactEmail} | 
                        <strong>Datum:</strong> ${new Date(req.createdDate).toLocaleDateString('cs-CZ')}
                    </p>
                </div>
                <span class="status status-${req.status.toLowerCase().replace('ě', 'e').replace('ř', 'r')}">${req.status.toUpperCase()}</span>
            </div>
            
            <div class="request-content">
                <div class="request-details">
                    <p><strong>Požadavek:</strong> ${req.requestText}</p>
                    
                    ${req.assignedAdminEmail ? `
                    <div class="assigned-info">
                        <strong>Přiřazeno:</strong> ${req.assignedAdminFirstName} ${req.assignedAdminLastName} (${req.assignedAdminEmail})
                        ${req.startedDate ? `<br><small>Začátek: ${new Date(req.startedDate).toLocaleString('cs-CZ')}</small>` : ''}
                    </div>
                    ` : ''}
                    
                    ${req.endedDate ? `
                    <div class="completed-info">
                        <strong>Dokončeno:</strong> ${new Date(req.endedDate).toLocaleString('cs-CZ')}
                        ${req.responseText ? `<br><strong>Odpověď:</strong> ${req.responseText}</strong>` : ''}
                    </div>
                    ` : ''}
                </div>
                
                <div class="request-actions">
                    ${getRequestActions(req)}
                </div>
            </div>
        `;
        requestsList.appendChild(reqDiv);
    });
}

function getRequestActions(req) {
    const admin = getLoggedAdmin();
    if (!admin) return '';
    
    const isAssignedToMe = req.assignedAdminEmail === admin.email;
    const status = req.status.toLowerCase();

    debugLog(`getRequestActions - Status: ${status}, isAssignedToMe: ${isAssignedToMe}`, req);
    
    let actionsHTML = '';
    
    if (status === 'novy') {
        actionsHTML += `
            <button onclick="assignRequest(${req.requestId})" class="btn btn-assign">
                Přiřadit mě
            </button>
        `;
    }
    
    if ((status === "resise") && isAssignedToMe) {
        actionsHTML += `
            <button onclick="finishRequest(${req.requestId})" class="btn btn-finish">
                Dokončit
            </button>
            <button onclick="cancelRequest(${req.requestId})" class="btn btn-cancel">
                Zrušit
            </button>
        `;
    }
    
    if ((status === "resise") && !isAssignedToMe && req.assignedAdminEmail) {
        actionsHTML += `
            <span class="assigned-to-other">Přiřazeno jinému adminovi</span>
        `;
    }
    
    if (status === 'uzavreny' || status === 'storno') {
        actionsHTML += `
            <span class="view-only">${status === 'uzavreny' ? 'Dokončeno' : 'Zrušeno'}</span>
        `;
    }
    
    actionsHTML += `
        <button onclick="showRequestDetails(${req.requestId})" class="btn btn-details">
            Detail
        </button>
    `;
    
    return actionsHTML;
}

async function assignRequest(requestId) {
    const admin = getLoggedAdmin();
    if (!admin || !admin.id) {
        alert("Nejste přihlášen!");
        return;
    }
    
    if (!confirm("Opravdu chcete přiřadit tento požadavek sobě?")) {
        return;
    }
    
    try {
        debugLog(`Přiřazování requestu ${requestId} adminovi ${admin.id}`);
        await apiRequest(`/requests/${requestId}/assign`, "POST", { 
            adminId: admin.id 
        });
        alert("Požadavek byl přiřazen!");
        
        setTimeout(() => {
            loadRequests();
            loadAdminStatistics();
        }, 500);
        
    } catch (err) {
        debugLog("Chyba při přiřazování", err.message);
        alert("Chybný formát");
    }
}

async function finishRequest(requestId) {
    const admin = getLoggedAdmin();
    if (!admin || !admin.id) {
        alert("Nejste přihlášen!");
        return;
    }
    
    const responseText = prompt("Zadejte odpověď pro klienta:");
    if (!responseText || !responseText.trim()) {
        alert("Odpověď je povinná a nemůže být prázdná!");
        return;
    }
    
    try {
        debugLog(`Dokončování requestu ${requestId} pro admina ${admin.id}`);
        
        const result = await apiRequest(`/requests/${requestId}/finish`, "POST", { 
            adminId: admin.id,
            responseText: responseText
        });
        
        debugLog("Dokončení requestu úspěšné", result);
        alert("Požadavek byl úspěšně dokončen!");
        
        setTimeout(() => {
            loadRequests();
            loadAdminStatistics();
        }, 500);
        
    } catch (err) {
        debugLog("Chyba při dokončování", err);
        alert("Chybný formát");
    }
}

async function cancelRequest(requestId) {
    const admin = getLoggedAdmin();
    if (!admin || !admin.id) {
        alert("Nejste přihlášen!");
        return;
    }
    
    const reason = prompt("Zadejte důvod pro zrušení požadavku (volitelné):", "Klient si přál zrušit");
    
    try {
        debugLog(`Rušení requestu ${requestId} pro admina ${admin.id}`);
        
        const result = await apiRequest(`/requests/${requestId}/cancel`, "POST", { 
            adminId: admin.id
        });
        
        debugLog("Zrušení requestu úspěšné", result);
        alert("Požadavek byl úspěšně zrušen!" + (reason ? `\nDůvod: ${reason}` : ''));
        
        setTimeout(() => {
            loadRequests();
            loadAdminStatistics();
        }, 500);
        
    } catch (err) {
        debugLog("Chyba při rušení", err);
        alert("Chybný formát");
    }
}

async function showRequestDetails(requestId) {
    try {
        const requests = await apiRequest("/requests/overview", "GET");
        const request = requests.find(r => r.requestId === requestId);
        
        if (request) {
            const detailText = `
Požadavek #${request.requestId}
Status: ${request.status}
Klient: ${request.contactFirstName} ${request.contactLastName}
Email: ${request.contactEmail}
Datum vytvoření: ${new Date(request.createdDate).toLocaleString('cs-CZ')}
Požadavek: ${request.requestText}
${request.assignedAdminEmail ? `Přiřazeno: ${request.assignedAdminFirstName} ${request.assignedAdminLastName}` : 'Nepřiřazeno'}
${request.startedDate ? `Začátek zpracování: ${new Date(request.startedDate).toLocaleString('cs-CZ')}` : ''}
${request.endedDate ? `Konec zpracování: ${new Date(request.endedDate).toLocaleString('cs-CZ')}` : ''}
${request.responseText ? `Odpověď: ${request.responseText}` : ''}
            `;
            alert(detailText);
        } else {
            alert("Požadavek nebyl nalezen");
        }
    } catch (err) {
        debugLog("Chyba při načítání detailu", err.message);
        alert("Chybný formát");
    }
}

async function loadAdminStatistics() {
    try {
        const admin = getLoggedAdmin();
        if (!admin) return;
        
        debugLog("Načítání statistik admina", admin.id);
        const stats = await apiRequest(`/admin/${admin.id}/statistics`, "GET") || {};
        debugLog("Statistiky načteny", stats);
        
        const statsDiv = document.getElementById("adminStatistics");
        if (!statsDiv) return;
        
        statsDiv.innerHTML = `
            <div class="stats-header">
                <h3>Vaše statistiky</h3>
                <button onclick="refreshAll()" class="btn-refresh">Obnovit</button>
            </div>
            <div class="stats-grid">
                <div class="stat-item">
                    <span class="stat-value">${stats.newRequests ?? 0}</span>
                    <span class="stat-label">Nové</span>
                </div>
                <div class="stat-item">
                    <span class="stat-value">${stats.assignedRequests ?? 0}</span>
                    <span class="stat-label">Přiřazené</span>
                </div>
                <div class="stat-item">
                    <span class="stat-value">${stats.completedRequests ?? 0}</span>
                    <span class="stat-label">Dokončené</span>
                </div>
                <div class="stat-item">
                    <span class="stat-value">${stats.cancelledRequests ?? 0}</span>
                    <span class="stat-label">Zrušené</span>
                </div>
                <div class="stat-item">
                    <span class="stat-value">${stats.totalProcessed ?? 0}</span>
                    <span class="stat-label">Celkem</span>
                </div>
                <div class="stat-item">
                    <span class="stat-value">${stats.avgProcessingTimeMinutes ?? 0}</span>
                    <span class="stat-label">Prům. minuty</span>
                </div>
            </div>
        `;
        
    } catch (err) {
        debugLog("Chyba při načítání statistik", err.message);
    }
}

async function loadAdmins() {
    try {
        const container = document.getElementById("admins");
        if (!container) return;
        
        container.innerHTML = "<p>Načítám administrátory...</p>";
        
        debugLog("Načítání seznamu adminů");
        
        try {
            const allStats = await apiRequest("/admin/statistics", "GET");
            debugLog("Admin stats načteny", allStats ? allStats.length : 0);
            
            const registerForm = document.createElement("div");
            registerForm.className = "admin-register-form";
            registerForm.innerHTML = `
                <h3>Přidat admina</h3>
                <form id="registerAdminForm">
                    <input type="text" id="newFirstName" placeholder="Jméno" required>
                    <input type="text" id="newLastName" placeholder="Příjmení" required>
                    <input type="email" id="newEmail" placeholder="Email" required>
                    <button type="submit" class="btn btn-add">Přidat admina</button>
                </form>
            `;
            container.appendChild(registerForm);
            
            document.getElementById("registerAdminForm").addEventListener("submit", async function(e) {
                e.preventDefault();
                await registerNewAdmin();
            });
            
            const listDiv = document.createElement("div");
            listDiv.className = "admin-list";
            listDiv.innerHTML = "<h3>Seznam adminů</h3>";
            
            if (allStats && allStats.length > 0) {
                allStats.forEach(admin => {
                    const adminDiv = document.createElement("div");
                    adminDiv.className = "admin-item";
                    adminDiv.innerHTML = `
                        <div class="admin-info">
                            <strong>${admin.firstName} ${admin.lastName}</strong>
                            <div class="admin-details">
                                <small>${admin.email}</small>
                                <small>${admin.adminRole}</small>
                                <small>Zpracováno: ${admin.totalProcessed ?? 0}</small>
                            </div>
                        </div>
                        <div class="admin-actions">
                            <button onclick="updateAdmin(${admin.adminId})" class="btn btn-edit">Upravit</button>
                            <button onclick="deleteAdmin(${admin.adminId})" class="btn btn-delete">Smazat</button>
                        </div>
                    `;
                    listDiv.appendChild(adminDiv);
                });
            } else {
                listDiv.innerHTML += "<p>Žádní administrátoři</p>";
            }
            
            container.appendChild(listDiv);
            
        } catch (err) {
            debugLog("Chyba při načítání statistik adminů", err.message);
            container.innerHTML += `<p class="error">Chybný formát</p>`;
        }
        
    } catch (err) {
        debugLog("Chyba při načítání adminů", err.message);
        const container = document.getElementById("admins");
        if (container) {
            container.innerHTML = `<p class="error">Chybný formát</p>`;
        }
    }
}

async function registerNewAdmin() {
    const firstName = document.getElementById("newFirstName").value;
    const lastName = document.getElementById("newLastName").value;
    const email = document.getElementById("newEmail").value;
    
    if (!firstName || !lastName || !email) {
        alert("Vyplňte všechny údaje");
        return;
    }
    
    try {
        debugLog("Registrace nového admina z dashboardu", { firstName, lastName, email });
        const result = await apiRequest("/admin/register", "POST", {
            firstName: firstName,
            lastName: lastName,
            email: email
        });
        
        alert("Admin úspěšně registrován!");
        document.getElementById("registerAdminForm").reset();
        loadAdmins();
    } catch (err) {
        debugLog("Chyba registrace z dashboardu", err.message);
        alert("Chybný formát");
    }
}

async function updateAdmin(adminId) {
    const admin = getLoggedAdmin();
    if (!admin || admin.id !== adminId) {
        alert("Můžete upravovat pouze svůj vlastní účet!");
        return;
    }
    
    const newEmail = prompt("Zadejte nový email:", admin.email);
    if (!newEmail) return;
    
    const newFirstName = prompt("Zadejte nové jméno:", admin.firstName);
    if (!newFirstName) return;
    
    const newLastName = prompt("Zadejte nové příjmení:", admin.lastName);
    if (!newLastName) return;
    
    const releaseRequests = confirm("Uvolnit všechny přiřazené požadavky?");
    
    try {
        debugLog("Aktualizace admina", { adminId, newEmail, newFirstName, newLastName });
        await apiRequest(`/admin/${adminId}`, "PUT", {
            firstName: newFirstName,
            lastName: newLastName,
            email: newEmail,
            releaseActiveRequests: releaseRequests
        });
        
        admin.firstName = newFirstName;
        admin.lastName = newLastName;
        admin.email = newEmail;
        localStorage.setItem("admin", JSON.stringify(admin));
        
        alert("Admin úspěšně aktualizován!");
        window.location.reload();
    } catch (err) {
        debugLog("Chyba při aktualizaci admina", err.message);
        alert("Chybný formát");
    }
}

async function deleteAdmin(adminId) {
    const admin = getLoggedAdmin();
    if (!admin || admin.id !== adminId) {
        alert("Můžete smazat pouze svůj vlastní účet!");
        return;
    }
    
    if (!confirm("Opravdu chcete smazat svůj účet? Všechny přiřazené požadavky budou změněny zpět na 'Nové' a uvolněny pro další administrátory.")) {
        return;
    }
    
    try {
        debugLog("Mazání admina", adminId);
        await apiRequest(`/admin/${adminId}`, "DELETE");
        
        alert("Admin úspěšně smazán! Všechny přiřazené požadavky byly uvolněny.");
        localStorage.removeItem("admin");
        window.location.href = "index.html";
    } catch (err) {
        debugLog("Chyba při mazání admina", err.message);
        alert("Chybný formát");
    }
}

function refreshAll() {
    debugLog("Ruční obnovení všech dat");
    loadAdminStatistics();
    loadRequests();
    loadAdmins();
    alert("Data obnovena!");
}

const logoutBtn = document.getElementById("logout");
if (logoutBtn) {
    logoutBtn.addEventListener("click", () => {
        debugLog("Odhlášení uživatele");
        if (confirm("Opravdu se chcete odhlásit?")) {
            localStorage.removeItem("admin");
            window.location.href = "index.html";
        }
    });
}

window.assignRequest = assignRequest;
window.finishRequest = finishRequest;
window.cancelRequest = cancelRequest;
window.showRequestDetails = showRequestDetails;
window.loadRequestsByStatus = loadRequestsByStatus;
window.loadRequests = loadRequests;
window.updateAdmin = updateAdmin;
window.deleteAdmin = deleteAdmin;
window.refreshAll = refreshAll;