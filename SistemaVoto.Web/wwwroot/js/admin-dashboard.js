// wwwroot/js/admin-dashboard.js

// Seguridad
if (!verificarAutenticacion() || !verificarRol(['Administrador'])) {
    // auth.js debe redirigir si no cumple
}

document.addEventListener('DOMContentLoaded', async () => {
    await cargarNombreUsuario();
    await cargarDashboard();
});

// NAVEGACIÓN (tu HTML usa onclick)
function irUsuarios() {
    window.location.href = '/pages/Admin/usuarios.html';
}

function irAsignaciones() {
    window.location.href = '/pages/Admin/asignaciones.html';
}

function irIncidencias() {
    window.location.href = '/pages/Admin/incidencias.html';
}

function irLogs() {
    window.location.href = '/pages/Admin/logs.html';
}

// NUEVAS PÁGINAS
function irElecciones() {
    window.location.href = '/pages/Admin/elecciones.html';
}
function irListasPoliticas() {
    window.location.href = '/pages/Admin/listas-politicas.html';
}
function irCandidatos() {
    window.location.href = '/pages/Admin/candidatos.html';
}
function irUbicaciones() {
    window.location.href = '/pages/Admin/ubicaciones.html';
}

// Cerrar sesión (tu HTML llama cerrarSesion())
function cerrarSesion() {
    // si auth.js tiene logout(), úsalo
    if (typeof logout === 'function') return logout();

    // fallback
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/index.html';
}

// CARGA DASHBOARD
async function cargarDashboard() {
    try {
        const dash = await fetchAPI('/Admin/dashboard');

        renderResumen(dash?.resumen);
        renderActividad(dash?.ultimaActividad || []);

    } catch (error) {
        mostrarMensaje('mensaje', error?.message || 'Error al cargar dashboard', 'error');
        console.error(error);

        // UI fallback
        const resumen = document.getElementById('resumen');
        if (resumen) resumen.innerHTML = `<p style="color:red;">Error cargando resumen</p>`;
        const actividad = document.getElementById('actividad');
        if (actividad) actividad.innerHTML = `<p style="color:red;">Error cargando actividad</p>`;
    }
}

// NOMBRE USUARIO (arriba)
async function cargarNombreUsuario() {
    const el = document.getElementById('nombreUsuario');
    if (!el) return;

    // 1) intentar desde localStorage
    try {
        const raw = localStorage.getItem('user');
        if (raw) {
            const u = JSON.parse(raw);
            const nombre = u?.nombre || u?.Nombre || u?.email || u?.Email;
            if (nombre) {
                el.textContent = nombre;
                return;
            }
        }
    } catch (_) { }

    // 2) fallback: decodificar JWT (si existe)
    try {
        const token = localStorage.getItem('token');
        if (!token) return;
        const payload = JSON.parse(atob(token.split('.')[1]));
        el.textContent = payload?.name || payload?.email || 'Administrador';
    } catch (_) {
        el.textContent = 'Administrador';
    }
}

// RENDER RESUMEN
function renderResumen(r) {
    const cont = document.getElementById('resumen');
    if (!cont) return;

    if (!r) {
        cont.innerHTML = `<p style="color:#666;">Sin datos de resumen</p>`;
        return;
    }

    cont.innerHTML = `
        ${statCard("Usuarios", r.totalUsuarios)}
        ${statCard("Votantes", r.totalVotantes)}
        ${statCard("Jefes de Junta", r.totalJefes)}
        ${statCard("Elecciones", r.totalElecciones)}
        ${statCard("Elecciones Activas", r.eleccionesActivas)}
        ${statCard("Votos Emitidos", r.totalVotos)}
        ${statCard("Ubicaciones", r.totalUbicaciones)}
        ${statCard("Incidencias", r.totalIncidencias)}
        ${statCard("Pendientes", r.incidenciasPendientes)}
    `;
}

function statCard(label, value) {
    return `
        <div class="stat-card-modern">
            <div class="stat-number-modern">${value ?? 0}</div>
            <div class="stat-label-modern">${label}</div>
        </div>
    `;
}


// RENDER ACTIVIDAD (logs)
function renderActividad(logs) {
    const cont = document.getElementById('actividad');
    if (!cont) return;

    if (!logs.length) {
        cont.innerHTML = `<p style="color:#666;">Sin actividad reciente</p>`;
        return;
    }

    cont.innerHTML = `
    <table class="table">
      <thead>
        <tr>
          <th>Acción</th>
          <th>Descripción</th>
          <th>Usuario</th>
          <th>Fecha</th>
        </tr>
      </thead>
      <tbody>
        ${logs.map(l => `
          <tr>
            <td><strong>${esc(l.accion)}</strong></td>
            <td>${esc(l.descripcion || '')}</td>
            <td>${esc(l.usuario || 'Sistema')}</td>
            <td>${formatFecha(l.fecha)}</td>
          </tr>
        `).join('')}
      </tbody>
    </table>
  `;
}

// Utils
function num(v) {
    return (v === null || v === undefined) ? 0 : v;
}

function formatFecha(fechaISO) {
    if (!fechaISO) return '-';
    const d = new Date(fechaISO);
    return d.toLocaleString();
}

function esc(s) {
    return String(s ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}
