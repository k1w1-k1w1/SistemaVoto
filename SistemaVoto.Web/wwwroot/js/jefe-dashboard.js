(function () {
    if (!verificarAutenticacion()) return;
    if (!verificarRol(["JefeDeJunta"])) return;

    const usuario = obtenerUsuario();

    const userInfo = document.getElementById("userInfo");
    const estadoSesion = document.getElementById("estadoSesion");

    const btnLogout = document.getElementById("btnLogout");
    const btnVerificar = document.getElementById("btnVerificar");
    const btnGenerar = document.getElementById("btnGenerar");
    const btnIncidencia = document.getElementById("btnIncidencia");
    const btnRefrescar = document.getElementById("btnRefrescar");

    const recintoBox = document.getElementById("recintoBox");
    const recintoNote = document.getElementById("recintoNote");
    const statsGrid = document.getElementById("statsGrid");
    const codigosRecientes = document.getElementById("codigosRecientes");

    function setAccionesHabilitadas(habilitar) {
        btnVerificar.disabled = !habilitar;
        btnGenerar.disabled = !habilitar;
        btnIncidencia.disabled = !habilitar;
    }

    document.addEventListener("DOMContentLoaded", async () => {
        // encabezado
        const nombre = `${usuario?.nombre ?? ""} ${usuario?.apellido ?? ""}`.trim();
        userInfo.textContent = `Sesión activa: ${nombre || usuario?.email} • ${usuario?.email} • Rol: ${usuario?.rol}`;
        estadoSesion.textContent = "Autenticado correctamente";

        btnLogout.addEventListener("click", cerrarSesion);

        btnVerificar.addEventListener("click", () => {
            window.location.href = "/pages/jefe/verificar.html";
        });

        btnGenerar.addEventListener("click", () => {
            window.location.href = "/pages/jefe/generar-codigo.html";
        });

        btnIncidencia.addEventListener("click", () => {
            window.location.href = "/pages/jefe/reportar.html";
        });

        btnRefrescar.addEventListener("click", async () => {
            await cargarTodo();
            mostrarMensaje("mensaje", "✅ Datos actualizados", "exito");
        });

        await cargarTodo();
    });

    async function cargarTodo() {
        await cargarEstadisticas();
        await cargarCodigosHoy();
    }

    async function cargarEstadisticas() {
        try {
            setAccionesHabilitadas(false);
            statsGrid.innerHTML = `<p>Cargando estadísticas...</p>`;
            recintoBox.innerHTML = `Cargando información del recinto...`;

            const data = await fetchAPI("/JefeDeJunta/estadisticas-mi-recinto");

            // Si tu API devuelve algo tipo: { asignada: false, mensaje: "..."}
            // lo soportamos:
            if (data?.asignada === false) {
                recintoBox.innerHTML = `
          <div><b>Estado:</b> No asignado</div>
          <div>${data.mensaje || "No tienes un recinto asignado."}</div>
        `;
                recintoNote.textContent = "No puedes continuar hasta que un administrador te asigne un recinto.";
                statsGrid.innerHTML = `<p style="color:#999;">Sin estadísticas (sin recinto asignado)</p>`;
                setAccionesHabilitadas(false);
                return;
            }

            const u = data.ubicacion;
            const e = data.estadisticas;

            // Si por alguna razón no llega ubicacion, tratamos como no asignado
            if (!u) {
                recintoBox.innerHTML = `
          <div><b>Estado:</b> No asignado</div>
          <div>No se encontró información de recinto.</div>
        `;
                recintoNote.textContent = "No puedes continuar hasta que un administrador te asigne un recinto.";
                statsGrid.innerHTML = `<p style="color:#999;">Sin estadísticas</p>`;
                setAccionesHabilitadas(false);
                return;
            }

            // Recinto info
            recintoBox.innerHTML = `
        <div><b>Recinto:</b> ${u.nombre}</div>
        <div><b>Dirección:</b> ${u.direccion}</div>
        <div><b>Mesa:</b> ${u.numeroMesa ?? "-"}</div>
      `;

            // Habilitar acciones si el recinto está activo (si existe esa propiedad)
            if (u.activo === false) {
                recintoNote.textContent = "Este recinto está inactivo. Contacta al administrador.";
                setAccionesHabilitadas(false);
            } else {
                recintoNote.textContent = "Verifica al votante como presente y luego genera su código.";
                setAccionesHabilitadas(true);
            }

            // Stats cards (con tu grid)
            statsGrid.innerHTML = `
        <div class="stat-item">
          <h4>👥 Asignados</h4>
          <p class="stat-number">${num(e?.totalVotantesAsignados)}</p>
        </div>

        <div class="stat-item">
          <h4>✓ Presentes</h4>
          <p class="stat-number">${num(e?.votantesPresentes)}</p>
        </div>

        <div class="stat-item">
          <h4>🗳️ Votos</h4>
          <p class="stat-number">${num(e?.votosEmitidos)}</p>
        </div>

        <div class="stat-item">
          <h4>🔑 Códigos</h4>
          <p class="stat-number">${num(e?.codigosGenerados)}</p>
        </div>

        <div class="stat-item">
          <h4>📊 Participación</h4>
          <p class="stat-number">${num(e?.porcentajeParticipacion)}%</p>
        </div>
      `;
        } catch (err) {
            setAccionesHabilitadas(false);
            recintoBox.innerHTML = `<div><b>Error:</b> No se pudo cargar tu recinto.</div>`;
            statsGrid.innerHTML = `<p style="color:#c62828;">Error al cargar estadísticas: ${err.message || err}</p>`;
        }
    }

    async function cargarCodigosHoy() {
        try {
            codigosRecientes.innerHTML = `<div class="admin-spinner"></div>`;

            const data = await fetchAPI("/JefeDeJunta/codigos-hoy");

            if (!data?.codigos || data.codigos.length === 0) {
                codigosRecientes.innerHTML = `<p style="color:#666;">No hay códigos generados hoy.</p>`;
                return;
            }

            const rows = data.codigos.map(c => `
        <tr>
          <td><b>${esc(c.codigo)}</b></td>
          <td>${esc(c.cedula)}</td>
          <td>${esc(c.votante)}</td>
          <td>${esc(c.eleccion)}</td>
          <td>${formatFecha(c.fechaGeneracion)}</td>
          <td>
            <span class="badge-admin ${badgeEstadoCodigo(c.estado)}">${esc(c.estado)}</span>
          </td>
        </tr>
      `).join("");

            codigosRecientes.innerHTML = `
        <table class="admin-table">
          <thead>
            <tr>
              <th>Código</th>
              <th>Cédula</th>
              <th>Votante</th>
              <th>Elección</th>
              <th>Fecha</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>
      `;
        } catch (err) {
            codigosRecientes.innerHTML = `<p style="color:#c62828;">Error al cargar códigos: ${err.message || err}</p>`;
        }
    }

    function badgeEstadoCodigo(estado) {
        // ajusta si tus estados son distintos
        const s = String(estado || "").toLowerCase();
        if (s.includes("usado") || s.includes("consum")) return "badge-admin-inactive";
        if (s.includes("activo") || s.includes("vigente") || s.includes("generado")) return "badge-admin-active";
        return "badge-admin-pending";
    }

    function num(v) {
        return (v === null || v === undefined) ? 0 : v;
    }

    function formatFecha(iso) {
        if (!iso) return "-";
        try { return new Date(iso).toLocaleString(); } catch { return iso; }
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
