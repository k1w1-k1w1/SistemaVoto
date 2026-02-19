(function () {
    if (!verificarAutenticacion()) return;
    if (!verificarRol(["JefeDeJunta"])) return;

    let votanteSeleccionado = null;
    let eleccionSeleccionada = null;

    const paso1 = document.getElementById("paso1");
    const paso2 = document.getElementById("paso2");
    const paso3 = document.getElementById("paso3");

    const buscarForm = document.getElementById("buscarForm");
    const cedulaInput = document.getElementById("cedula");
    const eleccionSelect = document.getElementById("eleccionId");

    const datosVotante = document.getElementById("datosVotante");
    const btnGenerar = document.getElementById("btnGenerar");

    const resultadoCodigo = document.getElementById("resultadoCodigo");
    const codigoDisplay = document.getElementById("codigoDisplay");

    document.addEventListener("DOMContentLoaded", async () => {
        await cargarElecciones();
    });

    async function cargarElecciones() {
        try {
            const data = await fetchAPI("/Elecciones/activas");

            eleccionSelect.innerHTML = `<option value="">Seleccione una elección...</option>`;

            if (!Array.isArray(data) || data.length === 0) {
                eleccionSelect.innerHTML = `<option value="">No hay elecciones activas</option>`;
                return;
            }

            data.forEach((e) => {
                const opt = document.createElement("option");
                opt.value = e.eleccionId; // ✅ coincide con tu controller
                opt.textContent = `${e.nombre} (${e.tipo})`;
                eleccionSelect.appendChild(opt);
            });
        } catch (err) {
            mostrarMensaje("mensaje", err.message || "Error al cargar elecciones", "error");
        }
    }

    function mostrarPaso(n) {
        paso1.classList.toggle("oculto", n !== 1);
        paso2.classList.toggle("oculto", n !== 2);
        paso3.classList.toggle("oculto", n !== 3);
    }

    // Paso 1: Buscar/Verificar
    buscarForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const cedula = (cedulaInput.value || "").trim();
        const eleccionId = parseInt(eleccionSelect.value, 10);

        if (!cedula || cedula.length !== 10) {
            mostrarMensaje("mensaje", "Ingresa una cédula válida (10 dígitos).", "error");
            return;
        }
        if (!eleccionId) {
            mostrarMensaje("mensaje", "Selecciona una elección.", "error");
            return;
        }

        try {
            const data = await fetchAPI("/JefeDeJunta/verificar-votante", {
                method: "POST",
                body: JSON.stringify({ cedula, eleccionId }),
            });

            votanteSeleccionado = data?.votante || null;
            eleccionSeleccionada = eleccionId;

            if (!votanteSeleccionado?.votanteId) {
                mostrarMensaje("mensaje", "No se recibió votanteId desde la API.", "error");
                return;
            }

            // Si ya existe código, tu backend lo manda en tieneCodigoGenerado/codigoExistente
            if (votanteSeleccionado.tieneCodigoGenerado) {
                mostrarMensaje(
                    "mensaje",
                    `⚠️ Ya existe un código generado: ${votanteSeleccionado.codigoExistente || "(sin mostrar)"}. Puedes re-enviar generando otra vez.`,
                    "error"
                );
                // Igual mostramos el paso 2 para permitir reenviar si quieres
            }

            datosVotante.innerHTML = `
        <div class="votante-info">
          <p><strong>Nombre:</strong> ${votanteSeleccionado.nombreCompleto}</p>
          <p><strong>Cédula:</strong> ${votanteSeleccionado.cedula}</p>
          <p class="alert-info">📧 El código se enviará por Email y/o SMS si están configurados</p>
        </div>
      `;

            mostrarPaso(2);
            mostrarMensaje("mensaje", data?.mensaje || "Votante verificado.", "exito");
        } catch (err) {
            mostrarMensaje("mensaje", err.message || "Error al verificar votante", "error");
            mostrarPaso(1);
        }
    });

    // Paso 2: Generar
    btnGenerar.addEventListener("click", async () => {
        if (!votanteSeleccionado?.votanteId || !eleccionSeleccionada) {
            mostrarMensaje("mensaje", "Faltan datos. Vuelve a buscar al votante.", "error");
            mostrarPaso(1);
            return;
        }

        try {
            btnGenerar.disabled = true;

            const res = await fetchAPI("/JefeDeJunta/generar-codigo", {
                method: "POST",
                body: JSON.stringify({
                    votanteId: votanteSeleccionado.votanteId, // ✅ aquí está la clave
                    eleccionId: eleccionSeleccionada,
                }),
            });

            resultadoCodigo.innerHTML = `<p>✅ ${res?.mensaje || "Código generado"}</p>`;

            codigoDisplay.innerHTML = res?.codigo
                ? `<div class="codigo-grande">${res.codigo}</div>
           <p class="codigo-nota">Email enviado: ${res.emailEnviado ? "Sí" : "No"} | SMS: ${res.smsEnviado ? "Sí" : "No"}</p>`
                : `<p>No se recibió el código desde la API.</p>`;

            mostrarPaso(3);
        } catch (err) {
            mostrarMensaje("mensaje", err.message || "Error al generar código", "error");
        } finally {
            btnGenerar.disabled = false;
        }
    });
})();
