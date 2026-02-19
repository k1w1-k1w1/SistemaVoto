(function () {
    if (!verificarAutenticacion()) return;
    if (!verificarRol(["JefeDeJunta"])) return;

    let eleccionSeleccionada = null;

    const usuario = obtenerUsuario();

    const userInfo = document.getElementById("userInfo");
    const estadoSesion = document.getElementById("estadoSesion");

    const btnVolver = document.getElementById("btnVolver");
    const btnLogout = document.getElementById("btnLogout");
    const btnLimpiar = document.getElementById("btnLimpiar");

    const form = document.getElementById("verificarForm");
    const inputCedula = document.getElementById("cedula");
    const selectEleccion = document.getElementById("eleccionId");

    const resultado = document.getElementById("resultado");
    const accionesVotante = document.getElementById("accionesVotante");
    const btnGenerarCodigo = document.getElementById("btnGenerarCodigo");
    const notaResultado = document.getElementById("notaResultado");

    document.addEventListener("DOMContentLoaded", async () => {
        const nombre = `${usuario?.nombre ?? ""} ${usuario?.apellido ?? ""}`.trim();
        userInfo.textContent = `Sesión activa: ${nombre || usuario?.email} • ${usuario?.email} • Rol: ${usuario?.rol}`;
        estadoSesion.textContent = "Autenticado correctamente";

        btnVolver.addEventListener("click", () => {
            window.location.href = "/pages/jefe/dashboard.html"; // ✅ minúscula
        });

        btnLogout.addEventListener("click", cerrarSesion);

        btnLimpiar.addEventListener("click", () => {
            inputCedula.value = "";
            selectEleccion.value = "";
            accionesVotante.style.display = "none";
            resultado.innerHTML = "Ingresa la cédula y selecciona una elección para verificar.";
            notaResultado.textContent = "Aquí verás el estado del votante y si ya tiene código generado.";
        });

        // auto: solo números
        inputCedula.addEventListener("input", () => {
            inputCedula.value = inputCedula.value.replace(/\D/g, "").slice(0, 10);
        });

        await cargarEleccionesActivas();
    });

    // =========================
    // ELECCIONES (robusto)
    // 1) intenta /Elecciones/activas
    // 2) si falla, usa /Elecciones y filtra Activa
    // =========================
    async function cargarEleccionesActivas() {
        try {
            let data;
            try {
                data = await fetchAPI("/Elecciones/activas");
            } catch (e) {
                // fallback
                const all = await fetchAPI("/Elecciones");
                data = Array.isArray(all) ? all.filter(x => String(x.estado).toLowerCase() === "activa") : [];
            }

            selectEleccion.innerHTML = `<option value="">Seleccione una elección...</option>`;

            if (!data || data.length === 0) {
                selectEleccion.innerHTML = `<option value="">No hay elecciones activas</option>`;
                return;
            }

            data.forEach(e => {
                const opt = document.createElement("option");
                opt.value = e.eleccionId;
                opt.textContent = `${e.nombre} (${e.tipo})`;
                selectEleccion.appendChild(opt);
            });
        } catch (err) {
            selectEleccion.innerHTML = `<option value="">Error al cargar elecciones</option>`;
            mostrarMensaje("mensaje", err.message || "Error al cargar elecciones", "error");
        }
    }

    // =========================
    // VERIFICAR
    // =========================
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const cedula = inputCedula.value.trim();
        const eleccionId = parseInt(selectEleccion.value, 10);

        if (!/^\d{10}$/.test(cedula)) {
            mostrarMensaje("mensaje", "Ingresa una cédula válida (10 dígitos)", "error");
            return;
        }
        if (!eleccionId) {
            mostrarMensaje("mensaje", "Selecciona una elección activa", "error");
            return;
        }

        try {
            const data = await fetchAPI("/JefeDeJunta/verificar-votante", {
                method: "POST",
                body: JSON.stringify({ cedula, eleccionId }),
            });

            eleccionSeleccionada = eleccionId;

            mostrarMensaje("mensaje", data.mensaje || "Verificado", "exito");
            renderResultado(data.votante, data);
        } catch (err) {
            accionesVotante.style.display = "none";
            resultado.innerHTML = `<div><b>Error:</b> ${esc(err.message || "No se pudo verificar")}</div>`;
            notaResultado.textContent = "Revisa la cédula y la elección seleccionada.";
            mostrarMensaje("mensaje", err.message || "Error al verificar", "error");
        }
    });

    // =========================
    // RENDER RESULTADO
    // =========================
    function renderResultado(v, raw) {
        if (!v) {
            accionesVotante.style.display = "none";
            resultado.innerHTML = "No se encontró información del votante.";
            return;
        }

        // algunos campos pueden variar según tu API:
        const nombre = v.nombreCompleto || `${v.nombre ?? ""} ${v.apellido ?? ""}`.trim() || "-";
        const ubicacionTxt = v.ubicacion
            ? `${v.ubicacion} • Mesa ${v.mesa ?? "-"}`
            : (v.ubicacionNombre ? `${v.ubicacionNombre} • Mesa ${v.numeroMesa ?? "-"}` : "No asignada");

        const tieneCodigo = !!v.tieneCodigoGenerado;

        resultado.innerHTML = `
      <div><b>Nombre:</b> ${esc(nombre)}</div>
      <div><b>Cédula:</b> ${esc(v.cedula)}</div>
      <div><b>Email:</b> ${esc(v.email || "-")}</div>
      <div><b>Teléfono:</b> ${esc(v.telefono || "-")}</div>
      <div><b>Ubicación:</b> ${esc(ubicacionTxt)}</div>
      <div><b>Estado:</b> ${esc(v.estado || "-")}</div>
      ${tieneCodigo ? `
        <div style="margin-top:10px; padding:10px; border-radius:10px; background:#fff3cd; color:#856404;">
          ⚠️ Ya tiene código generado: <b>${esc(v.codigoExistente || "-")}</b>
        </div>
      ` : `
        <div style="margin-top:10px; padding:10px; border-radius:10px; background:#d4edda; color:#155724;">
          ✅ Listo para generar código (si ya está verificado como presente).
        </div>
      `}
    `;

        if (!tieneCodigo) {
            accionesVotante.style.display = "flex";

            btnGenerarCodigo.onclick = () => {
                // guardamos para la pantalla generar-codigo
                localStorage.setItem("votanteParaCodigo", JSON.stringify({
                    votanteId: v.votanteId,
                    eleccionId: eleccionSeleccionada,
                    nombreCompleto: nombre,
                    cedula: v.cedula
                }));

                window.location.href = "/pages/jefe/generar-codigo.html"; // ✅ minúscula
            };

            notaResultado.textContent = "Si el votante ya fue marcado como presente, puedes generar su código.";
        } else {
            accionesVotante.style.display = "none";
            notaResultado.textContent = "Este votante ya tiene un código generado.";
        }
    }

    function esc(s) {
        return String(s ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }
})();
