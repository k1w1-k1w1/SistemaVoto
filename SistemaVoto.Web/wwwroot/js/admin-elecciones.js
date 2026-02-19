// Verificar autenticación
if (!verificarAutenticacion() || !verificarRol(['Administrador'])) {
    // Redirige automáticamente
}

let eleccionEditando = null;

document.addEventListener('DOMContentLoaded', () => {
    cargarElecciones();
});

async function cargarElecciones() {
    try {
        const data = await fetchAPI('/Elecciones');

        if (data.length === 0) {
            document.getElementById('listaElecciones').innerHTML =
                '<p style="text-align: center; color: #666;">No hay elecciones registradas</p>';
            return;
        }

        const html = `
            <table class="admin-table">
                <thead>
                    <tr>
                        <th>Nombre</th>
                        <th>Tipo</th>
                        <th>Fecha Inicio</th>
                        <th>Fecha Fin</th>
                        <th>Estado</th>
                        <th>Candidatos</th>
                        <th>Votos</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    ${data.map(e => `
                        <tr>
                            <td><strong>${e.nombre}</strong></td>
                            <td>${e.tipo}</td>
                            <td>${new Date(e.fechaInicio).toLocaleDateString()}</td>
                            <td>${new Date(e.fechaFin).toLocaleDateString()}</td>
                            <td>
                                <span class="badge-admin ${getBadgeClass(e.estado)}">
                                    ${e.estado}
                                </span>
                            </td>
                            <td>${e.totalCandidatos}</td>
                            <td>${e.totalVotos}</td>
                            <td>
                                ${e.estado === 'Configuracion' ? `
                                    <button class="btn-admin-outline" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="editarEleccion(${e.eleccionId})">
                                        ✏️ Editar
                                    </button>
                                    <button class="btn-admin-success" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="iniciarEleccion(${e.eleccionId})">
                                        ▶️ Iniciar
                                    </button>
                                ` : ''}
                                ${e.estado === 'Activa' ? `
                                    <button class="btn-admin-primary" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="finalizarEleccion(${e.eleccionId})">
                                        ⏹️ Finalizar
                                    </button>
                                ` : ''}
                                ${e.estado === 'Finalizada' ? `
                                    <button class="btn-admin-outline" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="verResultados(${e.eleccionId})">
                                        📊 Resultados
                                    </button>
                                ` : ''}
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        document.getElementById('listaElecciones').innerHTML = html;

    } catch (error) {
        document.getElementById('listaElecciones').innerHTML =
            '<p style="text-align: center; color: red;">Error al cargar elecciones</p>';
        console.error('Error:', error);
    }
}

function getBadgeClass(estado) {
    const badges = {
        'Configuracion': 'badge-admin-pending',
        'Activa': 'badge-admin-active',
        'Finalizada': 'badge-admin-inactive',
        'Cancelada': 'badge-admin-inactive'
    };
    return badges[estado] || 'badge-admin-pending';
}

function abrirModalCrear() {
    eleccionEditando = null;
    document.getElementById('modalTitulo').textContent = 'Nueva Elección';
    document.getElementById('formEleccion').reset();
    document.getElementById('eleccionId').value = '';
    document.getElementById('modalEleccion').style.display = 'block';
}

async function editarEleccion(id) {
    try {
        const data = await fetchAPI(`/Elecciones/${id}`);

        eleccionEditando = data;
        document.getElementById('modalTitulo').textContent = 'Editar Elección';
        document.getElementById('eleccionId').value = data.eleccionId;
        document.getElementById('nombre').value = data.nombre;
        document.getElementById('descripcion').value = data.descripcion || '';
        document.getElementById('tipo').value = data.tipo;
        document.getElementById('fechaInicio').value = formatDateForInput(data.fechaInicio);
        document.getElementById('fechaFin').value = formatDateForInput(data.fechaFin);

        document.getElementById('modalEleccion').style.display = 'block';

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cargar elección', 'error');
    }
}

function formatDateForInput(dateString) {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function cerrarModal() {
    document.getElementById('modalEleccion').style.display = 'none';
}

document.getElementById('formEleccion').addEventListener('submit', async (e) => {
    e.preventDefault();

    const datos = {
        nombre: document.getElementById('nombre').value,
        descripcion: document.getElementById('descripcion').value,
        tipo: document.getElementById('tipo').value,
        fechaInicio: new Date(document.getElementById('fechaInicio').value).toISOString(),
        fechaFin: new Date(document.getElementById('fechaFin').value).toISOString()
    };

    try {
        const eleccionId = document.getElementById('eleccionId').value;

        if (eleccionId) {
            // Editar
            await fetchAPI(`/Elecciones/${eleccionId}`, {
                method: 'PUT',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Elección actualizada exitosamente', 'exito');
        } else {
            // Crear
            await fetchAPI('/Elecciones', {
                method: 'POST',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Elección creada exitosamente', 'exito');
        }

        cerrarModal();
        cargarElecciones();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al guardar elección', 'error');
    }
});

async function iniciarEleccion(id) {
    if (!confirm('¿Está seguro de iniciar esta elección? Una vez iniciada, no se podrán agregar más candidatos.')) {
        return;
    }

    try {
        await fetchAPI(`/Elecciones/${id}/iniciar`, {
            method: 'POST'
        });

        mostrarMensaje('mensaje', '✅ Elección iniciada exitosamente', 'exito');
        cargarElecciones();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al iniciar elección', 'error');
    }
}

async function finalizarEleccion(id) {
    if (!confirm('¿Está seguro de finalizar esta elección? Esta acción no se puede deshacer.')) {
        return;
    }

    try {
        await fetchAPI(`/Elecciones/${id}/finalizar`, {
            method: 'POST'
        });

        mostrarMensaje('mensaje', '✅ Elección finalizada exitosamente', 'exito');
        cargarElecciones();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al finalizar elección', 'error');
    }
}

async function verResultados(id) {
    try {
        const data = await fetchAPI(`/Elecciones/${id}/resultados`);

        let html = `
            <div class="admin-card" style="max-width: 800px; margin: 20px auto;">
                <h3>📊 Resultados - ${data.eleccion.nombre}</h3>
                <p><strong>Total de votos:</strong> ${data.totalVotos}</p>
                <table class="admin-table" style="margin-top: 20px;">
                    <thead>
                        <tr>
                            <th>Candidato</th>
                            <th>Lista Política</th>
                            <th>Votos</th>
                            <th>Porcentaje</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${data.resultados.map(r => `
                            <tr>
                                <td><strong>${r.nombreCompleto}</strong></td>
                                <td>${r.listaPolitica || 'Independiente'}</td>
                                <td>${r.cantidadVotos}</td>
                                <td>
                                    <div style="display: flex; align-items: center; gap: 10px;">
                                        <div style="flex: 1; background: #f0f0f0; border-radius: 10px; height: 20px; overflow: hidden;">
                                            <div style="background: #dc3545; height: 100%; width: ${r.porcentaje}%; transition: width 0.5s;"></div>
                                        </div>
                                        <span style="min-width: 50px;"><strong>${r.porcentaje}%</strong></span>
                                    </div>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;

        const modal = document.getElementById('modalEleccion');
        modal.querySelector('.admin-modal-content').innerHTML = `
            <div class="admin-modal-header">
                <h3>Resultados de la Elección</h3>
                <button class="admin-modal-close" onclick="cerrarModal(); cargarElecciones();">&times;</button>
            </div>
            ${html}
        `;
        modal.style.display = 'block';

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cargar resultados', 'error');
    }
}

// Cerrar modal al hacer clic fuera
window.onclick = function (event) {
    const modal = document.getElementById('modalEleccion');
    if (event.target === modal) {
        cerrarModal();
    }
}