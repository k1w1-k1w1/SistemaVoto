// Verificar autenticación
if (!verificarAutenticacion() || !verificarRol(['Administrador'])) {
    // Redirige automáticamente
}

let ubicacionEditando = null;

document.addEventListener('DOMContentLoaded', () => {
    cargarUbicaciones();
});

async function cargarUbicaciones() {
    try {
        const data = await fetchAPI('/Ubicaciones');

        if (data.length === 0) {
            document.getElementById('listaUbicaciones').innerHTML =
                '<p style="text-align: center; color: #666;">No hay ubicaciones registradas</p>';
            return;
        }

        const html = `
            <table class="admin-table">
                <thead>
                    <tr>
                        <th>Nombre</th>
                        <th>Dirección</th>
                        <th>Mesa</th>
                        <th>Capacidad</th>
                        <th>Votantes</th>
                        <th>Presentes</th>
                        <th>Votos</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    ${data.map(u => {
            const ocupacion = (u.totalVotantes / u.capacidadVotantes * 100).toFixed(0);
            return `
                        <tr>
                            <td><strong>${u.nombre}</strong></td>
                            <td>${u.direccion}</td>
                            <td>${u.numeroMesa}</td>
                            <td>${u.capacidadVotantes}</td>
                            <td>
                                ${u.totalVotantes} 
                                <small style="color: ${ocupacion > 90 ? '#dc3545' : '#666'};">
                                    (${ocupacion}%)
                                </small>
                            </td>
                            <td>${u.votantesPresentes}</td>
                            <td>${u.votosEmitidos}</td>
                            <td>
                                <span class="badge-admin ${u.activo ? 'badge-admin-active' : 'badge-admin-inactive'}">
                                    ${u.activo ? 'Activa' : 'Inactiva'}
                                </span>
                            </td>
                            <td>
                                <button class="btn-admin-outline" style="padding: 6px 12px; font-size: 12px;" 
                                    onclick="editarUbicacion(${u.ubicacionId})">
                                    ✏️ Editar
                                </button>
                                ${u.activo ? `
                                    <button class="btn-admin-secondary" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="toggleEstado(${u.ubicacionId}, false)">
                                        ❌ Desactivar
                                    </button>
                                ` : `
                                    <button class="btn-admin-success" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="toggleEstado(${u.ubicacionId}, true)">
                                        ✅ Activar
                                    </button>
                                `}
                                <button class="btn-admin-outline" style="padding: 6px 12px; font-size: 12px;" 
                                    onclick="verDetalles(${u.ubicacionId})">
                                    👁️ Ver
                                </button>
                            </td>
                        </tr>
                    `}).join('')}
                </tbody>
            </table>
        `;

        document.getElementById('listaUbicaciones').innerHTML = html;

    } catch (error) {
        document.getElementById('listaUbicaciones').innerHTML =
            '<p style="text-align: center; color: red;">Error al cargar ubicaciones</p>';
        console.error('Error:', error);
    }
}

function abrirModalCrear() {
    ubicacionEditando = null;
    document.getElementById('modalTitulo').textContent = 'Nueva Ubicación';
    document.getElementById('formUbicacion').reset();
    document.getElementById('ubicacionId').value = '';
    document.getElementById('modalUbicacion').style.display = 'block';
}

async function editarUbicacion(id) {
    try {
        const data = await fetchAPI(`/Ubicaciones/${id}`);

        ubicacionEditando = data;
        document.getElementById('modalTitulo').textContent = 'Editar Ubicación';
        document.getElementById('ubicacionId').value = data.ubicacionId;
        document.getElementById('nombre').value = data.nombre;
        document.getElementById('direccion').value = data.direccion;
        document.getElementById('numeroMesa').value = data.numeroMesa;
        document.getElementById('capacidadVotantes').value = data.capacidadVotantes;

        document.getElementById('modalUbicacion').style.display = 'block';

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cargar ubicación', 'error');
    }
}

function cerrarModal() {
    document.getElementById('modalUbicacion').style.display = 'none';
}

document.getElementById('formUbicacion').addEventListener('submit', async (e) => {
    e.preventDefault();

    const datos = {
        nombre: document.getElementById('nombre').value,
        direccion: document.getElementById('direccion').value,
        numeroMesa: document.getElementById('numeroMesa').value,
        capacidadVotantes: parseInt(document.getElementById('capacidadVotantes').value)
    };

    try {
        const ubicacionId = document.getElementById('ubicacionId').value;

        if (ubicacionId) {
            // Editar
            await fetchAPI(`/Ubicaciones/${ubicacionId}`, {
                method: 'PUT',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Ubicación actualizada exitosamente', 'exito');
        } else {
            // Crear
            await fetchAPI('/Ubicaciones', {
                method: 'POST',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Ubicación creada exitosamente', 'exito');
        }

        cerrarModal();
        cargarUbicaciones();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al guardar ubicación', 'error');
    }
});

async function toggleEstado(id, activar) {
    try {
        const endpoint = activar ? 'activar' : 'desactivar';
        await fetchAPI(`/Ubicaciones/${id}/${endpoint}`, {
            method: 'PATCH'
        });

        mostrarMensaje('mensaje', `✅ Ubicación ${activar ? 'activada' : 'desactivada'} exitosamente`, 'exito');
        cargarUbicaciones();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cambiar estado', 'error');
    }
}

async function verDetalles(id) {
    try {
        const data = await fetchAPI(`/Ubicaciones/${id}`);

        const html = `
            <div style="padding: 20px;">
                <h4 style="color: #dc3545; margin-bottom: 20px;">📍 ${data.nombre}</h4>
                
                <div style="margin-bottom: 20px;">
                    <p><strong>Dirección:</strong> ${data.direccion}</p>
                    <p><strong>Mesa:</strong> ${data.numeroMesa}</p>
                    <p><strong>Capacidad:</strong> ${data.capacidadVotantes} votantes</p>
                </div>

                <h5 style="color: #dc3545; margin-top: 20px;">👥 Votantes Asignados (${data.votantes.length})</h5>
                ${data.votantes.length > 0 ? `
                    <table class="admin-table" style="margin-top: 10px;">
                        <thead>
                            <tr>
                                <th>Nombre</th>
                                <th>Cédula</th>
                                <th>Estado</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${data.votantes.map(v => `
                                <tr>
                                    <td>${v.nombreCompleto}</td>
                                    <td>${v.cedula}</td>
                                    <td>
                                        <span class="badge-admin ${v.estado === 'VotoEmitido' ? 'badge-admin-active' : 'badge-admin-pending'}">
                                            ${v.estado}
                                        </span>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                ` : '<p style="color: #999;">No hay votantes asignados</p>'}

                <h5 style="color: #dc3545; margin-top: 20px;">👨‍💼 Jefes Asignados (${data.jefes.length})</h5>
                ${data.jefes.length > 0 ? `
                    <table class="admin-table" style="margin-top: 10px;">
                        <thead>
                            <tr>
                                <th>Nombre</th>
                                <th>Cédula</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${data.jefes.map(j => `
                                <tr>
                                    <td>${j.nombreCompleto}</td>
                                    <td>${j.cedula}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                ` : '<p style="color: #999;">No hay jefes asignados</p>'}
            </div>
        `;

        const modal = document.getElementById('modalUbicacion');
        modal.querySelector('.admin-modal-content').innerHTML = `
            <div class="admin-modal-header">
                <h3>Detalles de la Ubicación</h3>
                <button class="admin-modal-close" onclick="cerrarModal(); cargarUbicaciones();">&times;</button>
            </div>
            ${html}
        `;
        modal.style.display = 'block';

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cargar detalles', 'error');
    }
}

// Cerrar modal al hacer clic fuera
window.onclick = function (event) {
    const modal = document.getElementById('modalUbicacion');
    if (event.target === modal) {
        cerrarModal();
    }
}