// Verificar autenticación
if (!verificarAutenticacion() || !verificarRol(['Administrador'])) {
    // Redirige automáticamente
}

let listaEditando = null;

document.addEventListener('DOMContentLoaded', () => {
    cargarListas();
});

async function cargarListas() {
    try {
        const data = await fetchAPI('/ListasPoliticas');

        if (data.length === 0) {
            document.getElementById('listaListas').innerHTML =
                '<p style="text-align: center; color: #666;">No hay listas políticas registradas</p>';
            return;
        }

        const html = `
            <table class="admin-table">
                <thead>
                    <tr>
                        <th>Nombre</th>
                        <th>Siglas</th>
                        <th>Candidatos</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    ${data.map(l => `
                        <tr>
                            <td><strong>${l.nombre}</strong></td>
                            <td>${l.siglas || '-'}</td>
                            <td>${l.totalCandidatos}</td>
                            <td>
                                <span class="badge-admin ${l.activo ? 'badge-admin-active' : 'badge-admin-inactive'}">
                                    ${l.activo ? 'Activa' : 'Inactiva'}
                                </span>
                            </td>
                            <td>
                                <button class="btn-admin-outline" style="padding: 6px 12px; font-size: 12px;" 
                                    onclick="editarLista(${l.listaPoliticaId})">
                                    ✏️ Editar
                                </button>
                                ${l.activo ? `
                                    <button class="btn-admin-secondary" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="toggleEstado(${l.listaPoliticaId}, false)">
                                        ❌ Desactivar
                                    </button>
                                ` : `
                                    <button class="btn-admin-success" style="padding: 6px 12px; font-size: 12px;" 
                                        onclick="toggleEstado(${l.listaPoliticaId}, true)">
                                        ✅ Activar
                                    </button>
                                `}
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        document.getElementById('listaListas').innerHTML = html;

    } catch (error) {
        document.getElementById('listaListas').innerHTML =
            '<p style="text-align: center; color: red;">Error al cargar listas políticas</p>';
        console.error('Error:', error);
    }
}

function abrirModalCrear() {
    listaEditando = null;
    document.getElementById('modalTitulo').textContent = 'Nueva Lista Política';
    document.getElementById('formLista').reset();
    document.getElementById('listaId').value = '';
    document.getElementById('modalLista').style.display = 'block';
}

async function editarLista(id) {
    try {
        const data = await fetchAPI(`/ListasPoliticas/${id}`);

        listaEditando = data;
        document.getElementById('modalTitulo').textContent = 'Editar Lista Política';
        document.getElementById('listaId').value = data.listaPoliticaId;
        document.getElementById('nombre').value = data.nombre;
        document.getElementById('siglas').value = data.siglas || '';
        document.getElementById('logo').value = data.logo || '';
        document.getElementById('descripcion').value = data.descripcion || '';

        document.getElementById('modalLista').style.display = 'block';

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cargar lista política', 'error');
    }
}

function cerrarModal() {
    document.getElementById('modalLista').style.display = 'none';
}

document.getElementById('formLista').addEventListener('submit', async (e) => {
    e.preventDefault();

    const datos = {
        nombre: document.getElementById('nombre').value,
        siglas: document.getElementById('siglas').value || null,
        logo: document.getElementById('logo').value || null,
        descripcion: document.getElementById('descripcion').value || null
    };

    try {
        const listaId = document.getElementById('listaId').value;

        if (listaId) {
            // Editar
            await fetchAPI(`/ListasPoliticas/${listaId}`, {
                method: 'PUT',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Lista política actualizada exitosamente', 'exito');
        } else {
            // Crear
            await fetchAPI('/ListasPoliticas', {
                method: 'POST',
                body: JSON.stringify(datos)
            });
            mostrarMensaje('mensaje', '✅ Lista política creada exitosamente', 'exito');
        }

        cerrarModal();
        cargarListas();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al guardar lista política', 'error');
    }
});

async function toggleEstado(id, activar) {
    try {
        const endpoint = activar ? 'activar' : 'desactivar';
        await fetchAPI(`/ListasPoliticas/${id}/${endpoint}`, {
            method: 'PATCH'
        });

        mostrarMensaje('mensaje', `✅ Lista ${activar ? 'activada' : 'desactivada'} exitosamente`, 'exito');
        cargarListas();

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al cambiar estado', 'error');
    }
}

// Cerrar modal al hacer clic fuera
window.onclick = function (event) {
    const modal = document.getElementById('modalLista');
    if (event.target === modal) {
        cerrarModal();
    }
}