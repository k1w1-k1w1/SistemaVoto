(function () {
    if (!verificarAutenticacion()) return;
    if (!verificarRol(["Votante"])) return;

    const usuario = obtenerUsuario();
    if (!usuario) {
        cerrarSesion();
        return;
    }

    const estadoSesion = document.getElementById("estadoSesion");
    const subInfo = document.getElementById("subInfo");

    const btnVolver = document.getElementById("btnVolver");
    const btnLogout = document.getElementById("btnLogout");
    const btnImprimir = document.getElementById("btnImprimir");
    const btnIrDashboard = document.getElementById("btnIrDashboard");
    const btnLimpiar = document.getElementById("btnLimpiar");

    const fNombre = document.getElementById("fNombre");
    const fCedula = document.getElementById("fCedula");
    const fEleccion = document.getElementById("fEleccion");
    const fTipoEleccion = document.getElementById("fTipoEleccion");
    const fConfirmacion = document.getElementById("fConfirmacion");
    const fFecha = document.getElementById("fFecha");

    const estadoVotanteTxt = document.getElementById("estadoVotante");
    const eleccionIdTxt = document.getElementById("eleccionIdTxt");
    const votanteIdTxt = document.getElementById("votanteIdTxt");

    if (estadoSesion) estadoSesion.textContent = "Autenticado";
    if (subInfo) subInfo.textContent = `Votante: ${usuario.email || "—"}`;

    btnVolver?.addEventListener("click", () => (window.location.href = "/pages/votante/dashboard.html"));
    btnIrDashboard?.addEventListener("click", () => (window.location.href = "/pages/votante/dashboard.html"));
    btnLogout?.addEventListener("click", cerrarSesion);
    btnImprimir?.addEventListener("click", () => window.print());

    btnLimpiar?.addEventListener("click", () => {
        localStorage.removeItem("codigoVoto");
        localStorage.removeItem("codigoVotoId");
        localStorage.removeItem("eleccionId");
        localStorage.removeItem("eleccionNombre");
        localStorage.removeItem("certificadoId");
        localStorage.removeItem("numeroConfirmacion");
        mostrarMensaje("mensaje", "Datos del proceso limpiados.", "exito");
    });

    const eleccionId = parseInt(localStorage.getItem("eleccionId"), 10);

    if (!Number.isFinite(eleccionId)) {
        mostrarMensaje("mensaje", "No se encontró ElecciónId. Vuelve a la papeleta.", "error");
        setTimeout(() => (window.location.href = "/pages/votante/dashboard.html"), 900);
        return;
    }

    function setText(el, value) {
        if (!el) return;
        el.textContent = value === null || value === undefined || value === "" ? "—" : String(value);
    }

    async function cargarCertificado() {
        //ENDPOINT CORRECTO (según tu swagger)
        // GET /api/Votantes/mi-ubicacion
        const d = await fetchAPI(`/Votantes/mi-ubicacion`, { method: "GET" });

        const votanteId = d?.votanteId;
        const estadoVotante = d?.estadoVotante;

        if (!votanteId) throw new Error("No se pudo obtener el VotanteId desde /Votantes/mi-ubicacion.");

        setText(votanteIdTxt, votanteId);
        setText(eleccionIdTxt, eleccionId);
        setText(estadoVotanteTxt, estadoVotante || "—");

        const resp = await fetchAPI(`/Votantes/certificado/${votanteId}/${eleccionId}`, { method: "GET" });

        // tu backend puede devolver { certificado: {...} } o directamente el objeto
        const c = resp?.certificado || resp;

        setText(fNombre, c?.votante?.nombreCompleto);
        setText(fCedula, c?.votante?.cedula);
        setText(fEleccion, c?.eleccion?.nombre);
        setText(fTipoEleccion, c?.eleccion?.tipo);
        setText(fConfirmacion, c?.numeroConfirmacion);

        const fechaRaw = c?.fechaEmision;
        if (fechaRaw) {
            const dt = new Date(fechaRaw);
            setText(fFecha, dt.toLocaleString());
        } else {
            setText(fFecha, "—");
        }

        // Subtítulo
        if (subInfo) {
            subInfo.textContent = `Elección: ${c?.eleccion?.nombre ?? eleccionId} • Confirmación: ${c?.numeroConfirmacion ?? "—"}`;
        }
    }

    (async function init() {
        try {
            await cargarCertificado();
        } catch (e) {
            mostrarMensaje("mensaje", e.message || "No se pudo cargar el certificado.", "error");
            console.error(e);
        }
    })();
})();
