document.getElementById("requestForm").addEventListener("submit", async e => {
    e.preventDefault();

    const button = e.target.querySelector('.submit-button');

    try {
        button.innerHTML = 'Odesílám...';
        button.disabled = true;

        const data = {
            firstName: firstName.value,
            lastName: lastName.value,
            email: email.value,
            message: message.value
        };

        const res = await apiRequest("/requests", "POST", data);

        document.getElementById("result").innerText =
        res
            ? `Požadavek odeslán (ID: ${res.requestId})`
            : "Požadavek odeslán";
        
        document.getElementById("result").className = "success";
        document.getElementById("result").style.display = "block";
        
        e.target.reset();

    } catch (err) {
        document.getElementById("result").innerText = "Chybný formát";
        document.getElementById("result").className = "error";
        document.getElementById("result").style.display = "block";
        
    } finally {
        button.innerHTML = 'Odeslat požadavek';
        button.disabled = false;
    }
});