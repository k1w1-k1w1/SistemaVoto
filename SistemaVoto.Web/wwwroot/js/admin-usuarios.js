if (!verificarAutenticacion() || !verificarRol(['Administrador'])) { }

document.addEventListener('DOMContentLoaded', () => {
    cargarUsuarios();
});

function limpiarFiltros() {
    document.getElementById('fRol').value = '';
    document.getElementById('fEstado').value = '';
    document.getElementById('fBuscar').value = '';
    cargarUsuarios();
}

async function cargarUsuarios() {
    try {
        const rol = document.getElementById('fRol').value;
        const estado = document.getElementById('fEstado').value;
        const buscar = document.getElementById('fBuscar').value.trim();

        const params = new URLSearchParams();
        if (rol) params.append('rol', rol);
        if (estado) params.append('estado', estado);
        if (buscar) params.append('buscar', buscar);

        const data = await fetchAPI(`/Admin/usuarios?${params.toString()}`);

        const tbody = document.querySelector('#tablaUsuarios tbody');
        if (!data || data.length === 0) {
            tbody.innerHTML = `<tr><td colspan="7">Sin resultados</td></tr>`;
            return;
        }

        tbody.innerHTML = data.map(u => `
      <tr>
        <td>${u.usuarioId}</td>
        <td>${u.nombre} ${u.apellido}</td>
        <td>${u.cedula}</td>
        <td>${u.email}</td>
        <td>${u.rol}</td>
        <td>${u.estado}</td>
        <td>
          <button class="btn-secondary" onclick="cambiarRol(${u.usuarioId}, '${u.rol}')">Rol</button>
          <button class="btn-secondary" onclick="cambiarEstado(${u.usuarioId}, '${u.estado}')">Estado</button>
        </td>
      </tr>
    `).join('');

    } catch (e) {
        mostrarMensaje('mensaje', e.message || 'Error al cargar usuarios', 'error');
    }
}

async function cambiarRol(usuarioId, rolActual) {
    const nuevoRol = prompt(`Rol actual: ${rolActual}\nNuevo rol (Votante/JefeDeJunta/Administrador):`);
    if (!nuevoRol) return;

    try {
        await fetchAPI(`/Admin/usuarios/${usuarioId}/cambiar-rol`, {
            method: 'PUT',
            body: JSON.stringify({ nuevoRol })
        });
        mostrarMensaje('mensaje', '✅ Rol actualizado', 'exito');
        cargarUsuarios();
    } catch (e) {
        mostrarMensaje('mensaje', e.message || 'Error cambiando rol', 'error');
    }
}

async function cambiarEstado(usuarioId, estadoActual) {
    const nuevoEstado = prompt(`Estado actual: ${estadoActual}\nNuevo estado (PendienteVerificacion/Activa/Bloqueada/Suspendida):`);
    if (!nuevoEstado) return;

    try {
        await fetchAPI(`/Admin/usuarios/${usuarioId}/cambiar-estado`, {
            method: 'PUT',
            body: JSON.stringify({ nuevoEstado })
        });
        mostrarMensaje('mensaje', '✅ Estado actualizado', 'exito');
        cargarUsuarios();
    } catch (e) {
        mostrarMensaje('mensaje', e.message || 'Error cambiando estado', 'error');
    }
}
