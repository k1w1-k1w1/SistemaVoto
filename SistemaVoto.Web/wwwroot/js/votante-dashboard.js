(function () {

    // ✅ Seguridad
    if (!verificarAutenticacion()) return;
    if (!verificarRol(["Votante"])) return;

    // UI refs
    const userInfo = document.getElementById("userInfo");
    const estadoSesion = document.getElementById("estadoSesion");
    const ubicacionBox = document.getElementById("ubicacionBox");
    const ubicacionNote = document.getElementById("ubicacionNote");

    const btnLogout = document.getElementById("btnLogout");
    const btnCodigo = document.getElementById("btnCodigo");
    const btnMaps = document.getElementById("btnMaps");
    const btnLimpiar = document.getElementById("btnLimpiar");

    // UI extra (nuevo layout)
    const badgeAsignacion = document.getElementById("badgeAsignacion");
    const estadoVotanteUI = document.getElementById("estadoVotanteUI");
    const mesaUI = document.getElementById("mesaUI");
    const recintoUI = document.getElementById("recintoUI");
    const accionUI = document.getElementById("accionUI");

    const usuario = obtenerUsuario();

    if (!usuario || !usuario.usuarioId) {
        cerrarSesion();
        return;
    }

    // Mostrar usuario arriba
    const nombre = `${usuario.nombre ?? ""} ${usuario.apellido ?? ""}`.trim();
    userInfo.textContent = `Sesión: ${nombre || usuario.email} • ${usuario.email} • Rol: ${usuario.rol}`;
    estadoSesion.textContent = "Autenticado";

    // Helpers UI
    function setBotonCodigoLock(texto) {
        btnCodigo.disabled = true;
        btnCodigo.textContent = texto || "🔒 Debes presentarte en el recinto";
    }

    function setBotonCodigoUnlock() {
        btnCodigo.disabled = false;
        btnCodigo.textContent = "🔑 Ingresar código";
    }

    function setBadge(text, type = "neutral") {
        if (!badgeAsignacion) return;
        badgeAsignacion.textContent = text;

        // clases rápidas inline (sin depender de CSS externo)
        badgeAsignacion.className = "mini-badge " + type;
    }

    function setEstadoCards({ estadoVotante, mesa, recinto, accion }) {
        if (estadoVotanteUI) estadoVotanteUI.textContent = estadoVotante ?? "—";
        if (mesaUI) mesaUI.textContent = mesa ?? "—";
        if (recintoUI) recintoUI.textContent = recinto ?? "—";
        if (accionUI) accionUI.textContent = accion ?? "—";
    }

    // ✅ Cargar ubicación desde API
    async function cargarUbicacion() {
        try {
            const data = await fetchAPI(`/Votantes/mi-ubicacion`, { method: "GET" });

            // Si no hay body (null) o formato inesperado:
            if (!data) {
                setBadge("Sin respuesta", "warn");
                ubicacionBox.innerHTML = `<div><b>Error:</b> Respuesta vacía del servidor.</div>`;
                ubicacionNote.textContent = "Intenta recargar la página.";
                setBotonCodigoLock("🔒 Intenta nuevamente");
                btnMaps.disabled = true;
                return;
            }

            // No asignada
            if (!data.asignada) {
                setBadge("No asignado", "warn");

                ubicacionBox.innerHTML = `
                    <div><b>Estado:</b> No asignada</div>
                    <div>${data.mensaje || "No tienes recinto asignado."}</div>
                `;

                ubicacionNote.textContent = "No puedes continuar hasta que te asignen un recinto.";

                setEstadoCards({
                    estadoVotante: data.estadoVotante || "Sin asignación",
                    mesa: "—",
                    recinto: "—",
                    accion: "Esperar asignación del administrador"
                });

                setBotonCodigoLock("🔒 Debes esperar asignación");
                btnMaps.disabled = true;
                return;
            }

            // Asignada
            const u = data.ubicacion;
            setBadge("Asignado", "ok");

            localStorage.setItem("ubicacionId", u.ubicacionId);
            localStorage.setItem("ubicacionNombre", u.nombre);
            localStorage.setItem("ubicacionDireccion", u.direccion);
            localStorage.setItem("numeroMesa", u.numeroMesa || "");

            // Recinto inactivo
            if (u.activo === false) {
                setBadge("Recinto inactivo", "bad");

                ubicacionBox.innerHTML = `
                    <div><b>Recinto:</b> ${u.nombre}</div>
                    <div><b>Dirección:</b> ${u.direccion}</div>
                    <div><b>Mesa:</b> ${u.numeroMesa || "No asignada"}</div>
                    <div><b>Estado actual:</b> ${data.estadoVotante}</div>
                `;

                ubicacionNote.textContent = "Este recinto está inactivo. Contacta al administrador.";
                setEstadoCards({
                    estadoVotante: data.estadoVotante,
                    mesa: u.numeroMesa || "—",
                    recinto: u.nombre,
                    accion: "Contactar al administrador"
                });

                setBotonCodigoLock("🔒 Recinto inactivo");
                btnMaps.disabled = true;
                return;
            }

            // Render ubicación
            ubicacionBox.innerHTML = `
                <div><b>Recinto:</b> ${u.nombre}</div>
                <div><b>Dirección:</b> ${u.direccion}</div>
                <div><b>Mesa:</b> ${u.numeroMesa || "No asignada"}</div>
                <div><b>Estado votante:</b> ${data.estadoVotante}</div>
            `;

            // Google Maps
            const q = encodeURIComponent(u.direccion);
            btnMaps.disabled = false;
            btnMaps.onclick = () => window.open(`https://www.google.com/maps/search/?api=1&query=${q}`, "_blank");

            // ✅ Reglas habilitación + cambios de texto del botón
            if (data.estadoVotante === "VotoEmitido") {
                setBadge("Ya votó", "bad");
                setBotonCodigoLock("✅ Ya emitiste tu voto");
                ubicacionNote.textContent = "Ya emitiste tu voto. No puedes generar ni ingresar otro código.";

                setEstadoCards({
                    estadoVotante: "VotoEmitido",
                    mesa: u.numeroMesa || "—",
                    recinto: u.nombre,
                    accion: "Voto finalizado"
                });
            }
            else if (data.estadoVotante === "CodigoGenerado") {
                setBadge("Código listo", "ok");
                setBotonCodigoUnlock();
                ubicacionNote.textContent = "Tu código ya fue generado. Puedes continuar con el proceso.";

                setEstadoCards({
                    estadoVotante: "CodigoGenerado",
                    mesa: u.numeroMesa || "—",
                    recinto: u.nombre,
                    accion: "Ingresar código"
                });
            }
            else if (data.estadoVotante === "PresenteEnRecinto") {
                setBadge("Presente", "ok");
                setBotonCodigoUnlock();
                ubicacionNote.textContent = "Ya estás registrado como presente. Puedes ingresar tu código.";

                setEstadoCards({
                    estadoVotante: "PresenteEnRecinto",
                    mesa: u.numeroMesa || "—",
                    recinto: u.nombre,
                    accion: "Ingresar código"
                });
            }
            else {
                setBadge("Pendiente verificación", "warn");
                setBotonCodigoLock("🔒 Debes presentarte en el recinto");
                ubicacionNote.textContent = "Debes presentarte físicamente en el recinto para que te habiliten el código.";

                setEstadoCards({
                    estadoVotante: data.estadoVotante || "Registrado",
                    mesa: u.numeroMesa || "—",
                    recinto: u.nombre,
                    accion: "Presentarse al recinto"
                });
            }

        } catch (error) {
            setBadge("Error", "bad");

            ubicacionBox.innerHTML = `<div><b>Error:</b> No se pudo cargar la ubicación.</div>`;
            ubicacionNote.textContent = error.message || "Error al consultar la ubicación.";

            setEstadoCards({
                estadoVotante: "—",
                mesa: "—",
                recinto: "—",
                accion: "Reintentar"
            });

            setBotonCodigoLock("🔒 Error al cargar");
            btnMaps.disabled = true;

            console.error(error);
        }
    }

    cargarUbicacion();

    // Eventos
    btnLogout.addEventListener("click", cerrarSesion);

    btnCodigo.addEventListener("click", () => {
        // Solo entra si está habilitado (unlock)
        window.location.href = "/pages/votante/codigo.html";
    });

    btnLimpiar.addEventListener("click", () => {
        localStorage.removeItem("codigoVoto");
        localStorage.removeItem("eleccionId");
        localStorage.removeItem("eleccionNombre");
        localStorage.removeItem("ubicacionId");
        localStorage.removeItem("ubicacionNombre");
        localStorage.removeItem("ubicacionDireccion");
        localStorage.removeItem("numeroMesa");

        mostrarMensaje("mensaje", "Datos locales limpiados.", "exito");
    });

})();
