if (!verificarAutenticacion() || !verificarRol(['Administrador'])) { }

document.addEventListener('DOMContentLoaded', () => {
    cargarLogs();
});

function limpiarFiltros() {
    document.getElementById('fAccion').value = '';
    document.getElementById('fUsuarioId').value = '';
    cargarLogs();
}

async function cargarLogs() {
    try {
        const accion = document.getElementById('fAccion').value.trim();
        const usuarioId = document.getElementById('fUsuarioId').value;

        const params = new URLSearchParams();
        if (accion) params.append('accion', accion);
        if (usuarioId) params.append('usuarioId', usuarioId);

        // pagina/cantidad opcional
        params.append('pagina', '1');
        params.append('cantidad', '20');

        const data = await fetchAPI(`/Admin/logs?${params.toString()}`);

        const logs = data.logs || [];
        const tbody = document.querySelector('#tablaLogs tbody');

        if (logs.length === 0) {
            tbody.innerHTML = `<tr><td colspan="5">Sin logs</td></tr>`;
            return;
        }

        tbody.innerHTML = logs.map(l => `
      <tr>
        <td>${l.logId}</td>
        <td>${l.accion}</td>
        <td>${l.descripcion}</td>
        <td>${new Date(l.fecha).toLocaleString()}</td>
        <td>${l.usuario ? l.usuario.nombreCompleto : '-'}</td>
      </tr>
    `).join('');

    } catch (e) {
        mostrarMensaje('mensaje', e.message || 'Error al cargar logs', 'error');
    }
}
