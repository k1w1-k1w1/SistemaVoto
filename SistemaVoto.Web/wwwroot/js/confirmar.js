(async function () {
    const estado = document.getElementById("estado");
    const linkLogin = document.getElementById("linkLogin");
    const loader = document.getElementById("loader"); // ✅ nuevo

    const params = new URLSearchParams(window.location.search);
    const email = params.get("email");
    const token = params.get("token");

    if (!email || !token) {
        if (loader) loader.style.display = "none"; // ✅ nuevo
        mostrarMensaje("mensaje", "Faltan parámetros de verificación (email/token).", "error");
        estado.textContent = "No se pudo confirmar la cuenta.";
        linkLogin.style.display = "inline-block";
        return;
    }

    try {
        const endpoint = `/Usuarios/confirmar?email=${encodeURIComponent(email)}&token=${encodeURIComponent(token)}`;
        const data = await fetchAPI(endpoint, { method: "GET" });

        if (loader) loader.style.display = "none"; // ✅ nuevo
        mostrarMensaje("mensaje", data.mensaje || "Cuenta confirmada.", "exito");
        estado.textContent = "✅ Tu cuenta fue confirmada. Ya puedes iniciar sesión.";
        linkLogin.style.display = "inline-block";
    } catch (error) {
        if (loader) loader.style.display = "none"; // ✅ nuevo
        mostrarMensaje("mensaje", error.message || "No se pudo confirmar la cuenta.", "error");
        estado.textContent = "❌ No se pudo confirmar la cuenta.";
        linkLogin.style.display = "inline-block";
    }
})();
