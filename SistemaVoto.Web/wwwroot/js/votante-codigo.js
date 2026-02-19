(function () {
    if (!verificarAutenticacion()) return;
    if (!verificarRol(["Votante"])) return;

    const usuario = obtenerUsuario();
    if (!usuario || !usuario.usuarioId) {
        cerrarSesion();
        return;
    }

    const estadoSesion = document.getElementById("estadoSesion");
    const subInfo = document.getElementById("subInfo");
    const recintoInfo = document.getElementById("recintoInfo");

    const inputEleccionId = document.getElementById("eleccionId");
    const inputCodigo = document.getElementById("codigo");

    const btnValidar = document.getElementById("btnValidar");
    const btnVolver = document.getElementById("btnVolver");
    const btnLogout = document.getElementById("btnLogout");

    estadoSesion.textContent = "Autenticado";
    subInfo.textContent = `Votante: ${usuario.email}`;

    btnVolver.addEventListener("click", () => {
        window.location.href = "/pages/votante/dashboard.html";
    });

    btnLogout.addEventListener("click", cerrarSesion);

    // Cargar ubicación desde API para obtener VOTANTEID real
    let votanteId = null;

    async function cargarDatosVotante() {
        try {
            const data = await fetchAPI(`/Votantes/mi-ubicacion`, { method: "GET" });

            // requiere que hayas agregado votanteId al endpoint
            votanteId = data.votanteId;

            const u = data.ubicacion;
            recintoInfo.innerHTML = `
        <div><b>Recinto:</b> ${u?.nombre ?? "-"}</div>
        <div><b>Dirección:</b> ${u?.direccion ?? "-"}</div>
        <div><b>Mesa:</b> ${u?.numeroMesa || "No asignada"}</div>
        <div><b>Estado:</b> ${data.estadoVotante ?? "-"}</div>
      `;

            // UX: si ya tienes un eleccionId guardado
            const savedEleccionId = localStorage.getItem("eleccionId");
            if (savedEleccionId) inputEleccionId.value = savedEleccionId;

        } catch (e) {
            recintoInfo.innerHTML = `<div><b>Error:</b> No se pudo cargar tus datos.</div>`;
        }
    }

    cargarDatosVotante();

    // Enter para verificar
    inputCodigo.addEventListener("keydown", (e) => {
        if (e.key === "Enter") btnValidar.click();
    });

    btnValidar.addEventListener("click", async () => {
        const codigo = (inputCodigo.value || "").trim();
        const eleccionId = parseInt(inputEleccionId.value, 10);


        if (!eleccionId || eleccionId <= 0) {
            mostrarMensaje("mensaje", "Ingresa un ElecciónId válido.", "error");
            return;
        }

        if (!codigo) {
            mostrarMensaje("mensaje", "Ingresa el código de voto.", "error");
            return;
        }

        btnValidar.disabled = true;
        btnValidar.textContent = "Verificando...";

        try {
            //  TU endpoint real:
            const resp = await fetchAPI("/Votantes/verificar-codigo", {
                method: "POST",
                body: JSON.stringify({
                    codigo: codigo,
                    eleccionId: eleccionId
                })

            });

            // Guardar para los siguientes pasos
            localStorage.setItem("codigoVoto", codigo);
            localStorage.setItem("codigoVotoId", resp.codigoVotoId);
            localStorage.setItem("eleccionId", resp.eleccion?.eleccionId ?? eleccionId);
            localStorage.setItem("eleccionNombre", resp.eleccion?.nombre ?? "");

            mostrarMensaje("mensaje", resp.mensaje || "Código válido.", "exito");

            setTimeout(() => {
                // próximo paso: mostrar papeleta de esa elección
                window.location.href = "/pages/votante/papeleta.html";
            }, 700);

        } catch (error) {
            mostrarMensaje("mensaje", error.message || "Código no válido.", "error");
        } finally {
            btnValidar.disabled = false;
            btnValidar.textContent = "✅ Verificar código";
        }
    });

})();
