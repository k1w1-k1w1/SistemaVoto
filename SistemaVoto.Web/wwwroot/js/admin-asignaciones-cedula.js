// ✅ Seguridad
if (!verificarAutenticacion() || !verificarRol(['Administrador'])) {
    // auth.js debe redirigir si no cumple
}

document.addEventListener('DOMContentLoaded', () => {
    cargarUbicaciones();
});

// =========================
// UI helpers
// =========================
function mostrarAlert(mensaje, tipo = 'success') {
    const el = document.getElementById('mensaje');
    if (!el) return;

    el.classList.remove('oculto');

    // Reusar estilos del theme
    el.className = 'admin-alert ' + (
        tipo === 'success' ? 'admin-alert-success'
            : tipo === 'warning' ? 'admin-alert-warning'
                : 'admin-alert-error'
    );

    el.textContent = mensaje;

    setTimeout(() => el.classList.add('oculto'), 5000);
}

function getUbicacionIdSeleccionada() {
    const sel = document.getElementById('ubicacionSelect');
    const id = parseInt(sel.value);
    return Number.isFinite(id) ? id : null;
}

function normalizarCedula(value) {
    return (value || '').trim();
}

// =========================
// Cargar ubicaciones (select)
// =========================
async function cargarUbicaciones() {
    const select = document.getElementById('ubicacionSelect');
    const info = document.getElementById('ubicacionInfo');

    try {
        select.innerHTML = `<option value="">Cargando...</option>`;
        if (info) info.textContent = '';

        const data = await fetchAPI('/AdminAsignaciones/ubicaciones');

        if (!Array.isArray(data) || data.length === 0) {
            select.innerHTML = `<option value="">No hay ubicaciones registradas</option>`;
            return;
        }

        // Solo activas primero, luego inactivas
        const ordenadas = [...data].sort((a, b) => (b.activo === true) - (a.activo === true));

        select.innerHTML = `<option value="">Seleccione una ubicación...</option>` + ordenadas.map(u => {
            const estado = u.activo ? '✅' : '❌';
            const mesa = u.numeroMesa ? ` - Mesa ${u.numeroMesa}` : '';
            return `<option value="${u.ubicacionId}">${estado} ${u.nombre}${mesa}</option>`;
        }).join('');

        select.onchange = () => {
            const id = getUbicacionIdSeleccionada();
            const u = ordenadas.find(x => x.ubicacionId === id);
            if (!u) {
                if (info) info.textContent = '';
                return;
            }
            if (info) {
                info.textContent = `${u.direccion} | Capacidad: ${u.capacidadVotantes} | Estado: ${u.activo ? 'Activa' : 'Inactiva'}`;
            }
        };

        // Seleccionar la primera activa automáticamente
        const primeraActiva = ordenadas.find(u => u.activo);
        if (primeraActiva) {
            select.value = String(primeraActiva.ubicacionId);
            select.onchange();
        }

    } catch (err) {
        console.error(err);
        select.innerHTML = `<option value="">Error cargando ubicaciones</option>`;
        mostrarAlert(err.message || 'Error cargando ubicaciones', 'error');
    }
}

// =========================
// Acciones VOTANTE
// =========================
async function asignarVotante() {
    const cedula = normalizarCedula(document.getElementById('cedulaVotante').value);
    const ubicacionId = getUbicacionIdSeleccionada();

    if (!cedula) return mostrarAlert('Ingresa la cédula del votante.', 'warning');
    if (!ubicacionId) return mostrarAlert('Selecciona una ubicación.', 'warning');

    try {
        const res = await fetchAPI('/AdminAsignaciones/asignar-votante-cedula', {
            method: 'POST',
            body: JSON.stringify({ cedula, ubicacionId })
        });

        mostrarAlert(res?.mensaje || '✅ Votante asignado correctamente.', 'success');
    } catch (err) {
        console.error(err);
        mostrarAlert(err.message || 'Error asignando votante', 'error');
    }
}

async function quitarVotante() {
    const cedula = normalizarCedula(document.getElementById('cedulaVotante').value);
    if (!cedula) return mostrarAlert('Ingresa la cédula del votante.', 'warning');

    try {
        const res = await fetchAPI('/AdminAsignaciones/quitar-votante-cedula', {
            method: 'POST',
            body: JSON.stringify({ cedula })
        });

        mostrarAlert(res?.mensaje || '✅ Ubicación removida del votante.', 'success');
    } catch (err) {
        console.error(err);
        mostrarAlert(err.message || 'Error quitando ubicación al votante', 'error');
    }
}

// =========================
// Acciones JEFE
// =========================
async function asignarJefe() {
    const cedula = normalizarCedula(document.getElementById('cedulaJefe').value);
    const ubicacionId = getUbicacionIdSeleccionada();

    if (!cedula) return mostrarAlert('Ingresa la cédula del jefe.', 'warning');
    if (!ubicacionId) return mostrarAlert('Selecciona una ubicación.', 'warning');

    try {
        const res = await fetchAPI('/AdminAsignaciones/asignar-jefe-cedula', {
            method: 'POST',
            body: JSON.stringify({ cedula, ubicacionId })
        });

        mostrarAlert(res?.mensaje || '✅ Jefe asignado correctamente.', 'success');
    } catch (err) {
        console.error(err);
        mostrarAlert(err.message || 'Error asignando jefe', 'error');
    }
}

async function quitarJefe() {
    const cedula = normalizarCedula(document.getElementById('cedulaJefe').value);
    if (!cedula) return mostrarAlert('Ingresa la cédula del jefe.', 'warning');

    try {
        const res = await fetchAPI('/AdminAsignaciones/quitar-jefe-cedula', {
            method: 'POST',
            body: JSON.stringify({ cedula })
        });

        mostrarAlert(res?.mensaje || '✅ Ubicación removida del jefe.', 'success');
    } catch (err) {
        console.error(err);
        mostrarAlert(err.message || 'Error quitando ubicación al jefe', 'error');
    }
}

// =========================
// Logout (si tu auth.js lo tiene)
// =========================
function cerrarSesion() {
    if (typeof logout === 'function') return logout();
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/index.html';
}
