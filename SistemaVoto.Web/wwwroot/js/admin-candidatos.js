// Verificar autenticación
if (!verificarAutenticacion() || !verificarRol(['Administrador'])) {
    // Redirige automáticamente
}

let candidatoEditando = null;

document.addEventListener('DOMContentLoaded', () => {
    cargarEleccionesParaFiltro();
    cargarListasParaFiltro();
    cargarEleccionesParaModal();
    cargarListasParaModal();
    cargarCandidatos();
});

async function cargarEleccionesParaFiltro() {
    try {
        const data = await fetchAPI('/Elecciones');
        const select = document.getElementById('filtroEleccion');
        select.innerHTML = '<option value="">Todas las elecciones</option>';
        data.forEach(e => {
            select.innerHTML += `<option value="${e.eleccionId}">${e.nombre}</option>`;
        });
    } catch (error) {
        console.error('Error:', error);
    }
}

async function cargarListasParaFiltro() {
    try {
        const data = await fetchAPI('/ListasPoliticas');
        const select = document.getElementById('filtroLista');
        select.innerHTML = '<option value="">Todas las listas</option>';
        data.forEach(l => {
            select.innerHTML += `<option value="${l.listaPoliticaId}">${l.nombre}</option>`;
        });
    } catch (error) {
        console.error('Error:', error);
    }
}

async function cargarEleccionesParaModal() {
    try {
        const data = await fetchAPI('/Elecciones');
        const select = document.getElementById('eleccionId');
        select.innerHTML = '<option value="">Seleccione...</option>';
        data.forEach(e => {
            select.innerHTML += `<option value="${e.eleccionId}">${e.nombre} (${e.estado})</option>`;
        });
    } catch (error) {
        console.error('Error:', error);
    }
}

async function cargarListasParaModal() {
    try {
        const data = await fetchAPI('/ListasPoliticas');
        const select = document.getElementById('listaPoliticaId');
        select.innerHTML = '<option value="">Sin lista política</option>';
        data.forEach(l => {
            select.innerHTML += `<option value="${l.listaPoliticaId}">${l.nombre}</option>`;
        });
    } catch (error) {
        console.error('Error:', error);
    }
}

async function cargarCandidatos() {
    try {
        const data = await fetchAPI('/Candidatos');

        // Aplicar filtros
        const filtroEleccion = document.getElementById('filtroEleccion').value;
        const filtroLista = document.getElementById('filtroLista').value;

        let candidatosFiltrados = data;

        if (filtroEleccion) {
            candidatosFiltrados = candidatosFiltrados.filter(c =>
                c.eleccion.eleccionId == filtroEleccion
            );
        }

        if (filtroLista) {
            candidatosFiltrados = candidatosFiltrados.filter(c =>
                c.listaPolitica && c.listaPolitica.listaPoliticaId == filtroLista
            );
        }

        if (candidatosFiltrados.length === 0) {
            document.getElementById('listaCandidatos').innerHTML =
                '<p style="text-align: center; color: #666;">No hay candidatos registrados</p>';
            return;
        }

        const html = `
            <table class="admin-table">
                <thead>
                    <tr>
                        <th>Nombre Completo</th>
                        <th>Cédula</th>
                        <th>Elección</th>
                        <th>Lista Política</th>
                        <th>Votos</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    ${candidatosFiltrados.map(c => `
                        <tr>
                            <td><strong>${c.nombreCompleto}</strong></td>
                            <td>${c.cedula}</td>
                            <td>${c.eleccion.nombre}</td>
                            <td>${c.listaPolitica ? c.listaPolitica.nombre : 'Independiente'}</td>
                            <td>${c.totalVotos}</td>
                            <td>
                                <span class="badge-admin ${c.activo ? 'badge-admin-active' : 'badge-admin-inactive'}">
                                    ${c.activo ? 'Activo' : 'Inactivo'}
                                </span>
                            </td>
                            <td>
                                ${c.eleccion.estado === 'Configuracion' ? `
                                    <button class="btn-admin-outline" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="editarCandidato(${c.candidatoId})">
                                        ✏️ Editar
                                    </button>
                                    ${c.activo ? `
                                        <button class="btn-admin-secondary" style="padding: 6px 12px; font-size: 12px;" 
                                            onclick="toggleEstado(${c.candidatoId}, false)">
                                            ❌ Desactivar
                                        </button>
                                    ` : `
                                        <button class="btn-admin-success" style="padding: 6px 12px; font-size: 12px;" 
                                            onclick="toggleEstado(${c.candidatoId}, true)">
                                            ✅ Activar
                                        </button>
                                    `}
                                ` : '<span style="color: #999;">Elección iniciada</span>'}
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        document.getElementById('listaCandidatos').innerHTML = html;

    } catch (error) {
        document.getElementById('listaCandidatos').innerHTML =
            '<p style="text-align: center; color: red;">Error al cargar candidatos</p>';
        console.error('Error:', error);
    }
}

function abrirModalCrear() {
    candidatoEditando = null;
    document.getElementById('modalTitulo').textContent = 'Nuevo Candidato';
    document.getElementById('formCandidato').reset();
    document.getElementById('candidatoId').value = '';
    document.getElementById('modalCandidato').style.display = 'block';
}

async function editarCandidato(id) {
    try {
        const data = await fetchAPI(`/Candidatos/${id}`);

        candidatoEditando = data;
        document.getElementById('modalTitulo').textContent = 'Editar Candidato';
        document.getElementById('candidatoId').value = data.candidatoId;
        document.getElementById('nombre').value = data.nombre;
        document.getElementById('apellido').value = data.apellido;
        document.getElementById('cedula').value = data.cedula;
        document.getElementById('eleccionId').value = data.eleccion.eleccionId;
        document.getElementById('listaPoliticaId').value = data.listaPolitica ? data.listaPolitica.listaPoliticaId : '';
        document.getElementById('foto').value = data.foto || '';
        document.getElementById('propuestas').value = data.propuestas || '';

        document.getElementById('modalCandidato').style.display = 'block';

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cargar candidato', 'error');
    }
}

function cerrarModal() {
    document.getElementById('modalCandidato').style.display = 'none';
}

document.getElementById('formCandidato').addEventListener('submit', async (e) => {
    e.preventDefault();

    const listaPoliticaId = document.getElementById('listaPoliticaId').value;

    const datos = {
        nombre: document.getElementById('nombre').value,
        apellido: document.getElementById('apellido').value,
        cedula: document.getElementById('cedula').value,
        eleccionId: parseInt(document.getElementById('eleccionId').value),
        listaPoliticaId: listaPoliticaId ? parseInt(listaPoliticaId) : null,
        foto: document.getElementById('foto').value || null,
        propuestas: document.getElementById('propuestas').value || null
    };

    try {
        const candidatoId = document.getElementById('candidatoId').value;

        if (candidatoId) {
            // Editar
            await fetchAPI(`/Candidatos/${candidatoId}`, {
                method: 'PUT',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Candidato actualizado exitosamente', 'exito');
        } else {
            // Crear
            await fetchAPI('/Candidatos', {
                method: 'POST',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Candidato creado exitosamente', 'exito');
        }

        cerrarModal();
        cargarCandidatos();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al guardar candidato', 'error');
    }
});

async function toggleEstado(id, activar) {
    try {
        const endpoint = activar ? 'activar' : 'desactivar';
        await fetchAPI(`/Candidatos/${id}/${endpoint}`, {
            method: 'PATCH'
        });

        mostrarMensaje('mensaje', `✅ Candidato ${activar ? 'activado' : 'desactivado'} exitosamente`, 'exito');
        cargarCandidatos();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cambiar estado', 'error');
    }
}

// Cerrar modal al hacer clic fuera
window.onclick = function (event) {
    const modal = document.getElementById('modalCandidato');
    if (event.target === modal) {
        cerrarModal();
    }
}