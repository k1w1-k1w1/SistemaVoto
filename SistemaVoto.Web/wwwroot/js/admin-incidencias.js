if (!verificarAutenticacion() || !verificarRol(['Administrador'])) { }

document.addEventListener('DOMContentLoaded', () => {
    cargarIncidencias();
});

function limpiarFiltros() {
    document.getElementById('fEstado').value = '';
    document.getElementById('fTipo').value = '';
    cargarIncidencias();
}

async function cargarIncidencias() {
    try {
        const estado = document.getElementById('fEstado').value;
        const tipo = document.getElementById('fTipo').value.trim();

        const params = new URLSearchParams();
        if (estado) params.append('estado', estado);
        if (tipo) params.append('tipo', tipo);

        const data = await fetchAPI(`/Admin/incidencias?${params.toString()}`);

        const tbody = document.querySelector('#tablaIncidencias tbody');
        if (!data || data.length === 0) {
            tbody.innerHTML = `<tr><td colspan="7">Sin incidencias</td></tr>`;
            return;
        }

        tbody.innerHTML = data.map(i => `
      <tr>
        <td>${i.incidenciaId}</td>
        <td>${i.tipo}</td>
        <td>${i.estado}</td>
        <td>${new Date(i.fechaReporte).toLocaleString()}</td>
        <td>${i.jefeDeJunta?.nombreCompleto || '-'}</td>
        <td>${i.votante?.nombreCompleto || '-'}</td>
        <td>
          <button class="btn-secondary" onclick="resolver(${i.incidenciaId})">Resolver</button>
        </td>
      </tr>
    `).join('');

    } catch (e) {
        mostrarMensaje('mensaje', e.message || 'Error al cargar incidencias', 'error');
    }
}

async function resolver(incidenciaId) {
    const estado = prompt('Estado (Resuelta/Rechazada):');
    if (!estado) return;

    const resolucion = prompt('Resolución (texto):') || '';

    try {
        await fetchAPI(`/Admin/incidencias/${incidenciaId}/resolver`, {
            method: 'PUT',
            body: JSON.stringify({ estado, resolucion })
        });

        mostrarMensaje('mensaje', '✅ Incidencia actualizada', 'exito');
        cargarIncidencias();
    } catch (e) {
        mostrarMensaje('mensaje', e.message || 'Error al resolver incidencia', 'error');
    }
}
