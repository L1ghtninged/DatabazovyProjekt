
async function loadConfig() {
    try {
        const response = await fetch('/api/config');
        const config = await response.json();
        
        document.getElementById('dbInfo').innerHTML = `
            <p><strong>Databáze:</strong> ${config.databaseName}</p>
            <p><strong>Server:</strong> ${config.dataSource}</p>
        `;
        
        document.getElementById('dbStatusText').textContent = 'OK';
    } catch (error) {
        document.getElementById('dbInfo').innerHTML = '<p class="error">Nelze načíst konfiguraci</p>';
        document.getElementById('dbStatusText').textContent = 'Chyba';
    }
}

async function startAPI() {
    try {
        await fetch('/api/start', { method: 'POST' });
        showMessage('API spuštěno', 'success');
        updateApiStatus('running');
    } catch (error) {
        showMessage('Chyba při startu API', 'error');
    }
}

async function stopAPI() {
    try {
        await fetch('/api/stop', { method: 'POST' });
        showMessage('API zastaveno', 'success');
        updateApiStatus('stopped');
    } catch (error) {
        showMessage('Chyba při zastavení API', 'error');
    }
}

async function exportDB() {
    try {
        const response = await fetch('/api/export', { method: 'POST' });
        if (response.ok) {
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'database_backup.sql';
            a.click();
            showMessage('Databáze exportována', 'success');
        }
    } catch (error) {
        showMessage('Chyba při exportu', 'error');
    }
}

function importModal() {
    document.getElementById('modal').classList.remove('hidden');
}

function closeModal() {
    document.getElementById('modal').classList.add('hidden');
}

async function uploadCSV() {
    const fileInput = document.getElementById('csvFile');
    const tableSelect = document.getElementById('tableSelect');
    
    if (!fileInput.files[0]) {
        showMessage('Vyberte soubor', 'error');
        return;
    }
    
    const formData = new FormData();
    formData.append('file', fileInput.files[0]);
    formData.append('table', tableSelect.value);
    
    try {
        const response = await fetch('/api/import', {
            method: 'POST',
            body: formData
        });
        
        if (response.ok) {
            showMessage('Import úspěšný', 'success');
            closeModal();
            fileInput.value = '';
        }
    } catch (error) {
        showMessage('Chyba při importu', 'error');
    }
}

function updateApiStatus(status) {
    const element = document.getElementById('apiStatus');
    const footer = document.getElementById('apiStatusText');
    
    if (status === 'running') {
        element.textContent = 'Běží';
        element.className = 'status running';
        footer.textContent = 'Běží';
    } else {
        element.textContent = 'Zastaveno';
        element.className = 'status stopped';
        footer.textContent = 'Zastaveno';
    }
}

function showMessage(text) {
    alert(text);
}

document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('year').textContent = new Date().getFullYear();
    
    loadConfig();
    
    fetch('http://localhost:8080/')
        .then(() => updateApiStatus('running'))
        .catch(() => updateApiStatus('stopped'));
});