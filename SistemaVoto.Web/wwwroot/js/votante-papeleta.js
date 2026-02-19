(function () {
    if (!verificarAutenticacion()) return;
    if (!verificarRol(["Votante"])) return;

    const usuario = obtenerUsuario();
    if (!usuario) return cerrarSesion();

    const setMsg = (txt) => mostrarMensaje("mensaje", txt, "error");

    // UI
    const estadoSesion = document.getElementById("estadoSesion");
    const subInfo = document.getElementById("subInfo");
    const pillEleccion = document.getElementById("pillEleccion");
    const pillTipo = document.getElementById("pillTipo");
    const pillEstado = document.getElementById("pillEstado");

    const btnVolver = document.getElementById("btnVolver");
    const btnLogout = document.getElementById("btnLogout");

    const tipoVotoSel = document.getElementById("tipoVoto");
    const contenedorListas = document.getElementById("contenedorListas");
    const btnConfirmar = document.getElementById("btnConfirmar");
    const btnConfirmar2 = document.getElementById("btnConfirmar2");

    const seleccionText = document.getElementById("seleccionText");
    const seleccionMeta = document.getElementById("seleccionMeta");
    const footerChoice = document.getElementById("footerChoice");
    const footerHint = document.getElementById("footerHint");

    estadoSesion.textContent = "Autenticado";
    subInfo.textContent = `Votante: ${usuario.email}`;

    btnVolver?.addEventListener("click", () => (window.location.href = "/pages/votante/dashboard.html"));
    btnLogout?.addEventListener("click", cerrarSesion);

    // Datos sesión
    const eleccionIdRaw = localStorage.getItem("eleccionId");
    const codigoVotoIdRaw = localStorage.getItem("codigoVotoId");
    const eleccionId = parseInt(eleccionIdRaw, 10);
    const codigoVotoId = parseInt(codigoVotoIdRaw, 10);
    const eleccionNombre = localStorage.getItem("eleccionNombre") || "";

    // 👇 DEBUG visible
    console.log("DEBUG localStorage:", { eleccionIdRaw, eleccionId, codigoVotoIdRaw, codigoVotoId });

    if (!Number.isFinite(eleccionId) || !Number.isFinite(codigoVotoId)) {
        setMsg(`Faltan datos o están inválidos. eleccionId=${eleccionIdRaw} codigoVotoId=${codigoVotoIdRaw}`);
        setTimeout(() => (window.location.href = "/pages/votante/codigo.html"), 1200);
        return;
    }

    pillEleccion.textContent = `Elección: ${eleccionNombre || eleccionId}`;
    pillEstado.textContent = "Estado: Cargando...";
    pillTipo.textContent = `Tipo voto: ${tipoVotoSel?.value || "Individual"}`;

    let votanteId = null;
    let papeletaData = null;

    let seleccion = {
        tipo: tipoVotoSel?.value || "Individual",
        candidatoId: null,
        listaPoliticaId: null,
        nombre: null,
    };

    function actualizarSeleccionUI() {
        pillTipo.textContent = `Tipo voto: ${seleccion.tipo}`;

        if (!seleccion.nombre && seleccion.tipo !== "Blanco") {
            seleccionText.textContent = "Sin selección";
            seleccionMeta.textContent = "Elige un candidato o lista, o vota en blanco.";
            footerChoice.textContent = "Sin selección";
            footerHint.textContent = "Selecciona una opción para continuar.";
            btnConfirmar.disabled = true;
            btnConfirmar2.disabled = true;
            return;
        }

        if (seleccion.tipo === "Blanco") {
            seleccionText.textContent = "Voto en blanco";
            seleccionMeta.textContent = "No se selecciona candidato ni lista.";
            footerChoice.textContent = "Voto en blanco";
            footerHint.textContent = "Pulsa confirmar para registrar tu voto.";
            btnConfirmar.disabled = false;
            btnConfirmar2.disabled = false;
            return;
        }

        seleccionText.textContent = seleccion.nombre;
        seleccionMeta.textContent =
            seleccion.tipo === "Individual" ? "Voto individual por candidato." : "Voto en plancha por lista política.";
        footerChoice.textContent = seleccion.nombre;
        footerHint.textContent = "Revisa tu selección y confirma.";
        btnConfirmar.disabled = false;
        btnConfirmar2.disabled = false;
    }

    async function cargarVotanteId() {
        const endpoint = "/Votantes/mi-ubicacion";
        try {
            const d = await fetchAPI(endpoint, { method: "GET" });
            console.log("DEBUG mi-ubicacion:", d);

            if (!d || !d.votanteId) throw new Error("La API no devolvió votanteId en /mi-ubicacion");
            votanteId = d.votanteId;

            pillEstado.textContent = `Estado: ${d.estadoVotante || "Listo"}`;
        } catch (e) {
            console.error("ERROR en", endpoint, e);
            throw new Error(`Falló ${endpoint} → ${e.message || e}`);
        }
    }

    async function cargarPapeleta() {
        const endpoint = `/Votantes/papeleta/${eleccionId}`;
        try {
            const d = await fetchAPI(endpoint, { method: "GET" });
            console.log("DEBUG papeleta:", d);

            papeletaData = d;
            if (!papeletaData) throw new Error("Respuesta vacía");
        } catch (e) {
            console.error("ERROR en", endpoint, e);
            throw new Error(`Falló ${endpoint} → ${e.message || e}`);
        }
    }

    function pintarListas() {
        contenedorListas.innerHTML = "";

        const listas = papeletaData?.candidatosPorLista || [];
        if (!listas.length) {
            contenedorListas.innerHTML = `<div style="color:#666;font-weight:700;">No hay candidatos disponibles.</div>`;
            return;
        }

        listas.forEach((item) => {
            const listaNombre = item.listaPolitica?.nombre || "Lista";
            const div = document.createElement("div");
            div.className = "listaCard";
            div.innerHTML = `<h4>${listaNombre}</h4><div class="candidatos"></div>`;

            const cands = div.querySelector(".candidatos");

            item.candidatos.forEach((c) => {
                const fullName = c.nombreCompleto || `${c.nombre} ${c.apellido}`.trim();
                const row = document.createElement("div");
                row.className = "candidato";
                row.innerHTML = `
          <span>${fullName}</span>
          <input type="radio" name="pick" data-kind="candidato" data-candidato="${c.candidatoId}" data-nombre="${fullName}">
        `;
                cands.appendChild(row);
            });

            contenedorListas.appendChild(div);
        });

        contenedorListas.querySelectorAll('input[name="pick"]').forEach((r) => {
            r.addEventListener("change", () => {
                seleccion.tipo = "Individual";
                seleccion.candidatoId = parseInt(r.dataset.candidato, 10);
                seleccion.listaPoliticaId = null;
                seleccion.nombre = r.dataset.nombre;
                actualizarSeleccionUI();
            });
        });
    }

    async function emitirVoto() {
        if (!votanteId) return setMsg("No se pudo obtener votanteId.");
        if (seleccion.tipo === "Individual" && !seleccion.candidatoId) return setMsg("Selecciona un candidato.");

        const payload = {
            codigoVotoId,
            votanteId,
            tipoVoto: seleccion.tipo,
            candidatoId: seleccion.tipo === "Individual" ? seleccion.candidatoId : null,
            listaPoliticaId: null,
        };

        const ok = confirm("⚠️ Confirmación final:\n\n¿Estás seguro de emitir tu voto? Esto no se puede deshacer.");
        if (!ok) return;

        try {
            const resp = await fetchAPI("/Votantes/emitir-voto", {
                method: "POST",
                body: JSON.stringify(payload),
            });

            mostrarMensaje("mensaje", resp.mensaje || "Voto emitido.", "exito");
            setTimeout(() => (window.location.href = "/pages/votante/certificado.html"), 900);
        } catch (e) {
            setMsg(e.message || "No se pudo emitir el voto.");
        }
    }

    btnConfirmar?.addEventListener("click", emitirVoto);
    btnConfirmar2?.addEventListener("click", emitirVoto);

    (async function init() {
        try {
            await cargarVotanteId();
            await cargarPapeleta();
            actualizarSeleccionUI();
            pintarListas();
        } catch (e) {
            setMsg(e.message || "Error cargando papeleta.");
            console.error(e);
        }
    })();
})();
