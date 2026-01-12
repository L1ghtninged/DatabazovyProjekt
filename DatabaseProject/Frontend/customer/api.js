const API_BASE = "http://localhost:8080/api";

async function apiRequest(url, method = "GET", body = null) {
    const options = {
        method,
        headers: { "Content-Type": "application/json" }
    };

    if (body) {
        options.body = JSON.stringify(body);
    }

    const response = await fetch(API_BASE + url, options);

    const text = await response.text();

    if (!response.ok) {
        throw new Error(text || "API error");
    }

    return text ? JSON.parse(text) : null;
}

