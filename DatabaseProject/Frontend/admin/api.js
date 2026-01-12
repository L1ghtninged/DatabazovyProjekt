const API_BASE = "http://localhost:8080/api";

async function apiRequest(url, method = "GET", body = null) {
    const options = {
        method,
        headers: { "Content-Type": "application/json" }
    };

    if (body) {
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(API_BASE + url, options);
        
        const text = await response.text();
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        if (!text || text.trim() === '' || text === "null") {
            return null;
        }

        try {
            return JSON.parse(text);
        } catch (e) {
            console.error("Invalid JSON response:", text);
            throw new Error("Chybný formát");
        }
    } catch (err) {
        if (err.name === 'TypeError' && err.message.includes('fetch')) {
            throw new Error("Chybný formát");
        }
        throw new Error("Chybný formát");
    }
}