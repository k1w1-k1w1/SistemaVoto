if (!verificarAutenticacion() || !verificarRol(['Administrador'])) { }

document.getElementById('formAsignarVotante').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
        const votanteId = parseInt(document.getElementById('votanteId').value);
        const ubicacionId = parseInt(document.getElementById('ubicacionIdV').value);

        await fetchAPI('/Admin/asignar-votante', {
            method: 'POST',
            body: JSON.stringify({ votanteId, ubicacionId })
        });

        mostrarMensaje('mensaje', '✅ Votante asignado', 'exito');
        e.target.reset();
    } catch (err) {
        mostrarMensaje('mensaje', err.message || 'Error al asignar votante', 'error');
    }
});

document.getElementById('formAsignarJefe').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
        const jefeDeJuntaId = parseInt(document.getElementById('jefeId').value);
        const ubicacionId = parseInt(document.getElementById('ubicacionIdJ').value);

        await fetchAPI('/Admin/asignar-jefe', {
            method: 'POST',
            body: JSON.stringify({ jefeDeJuntaId, ubicacionId })
        });

        mostrarMensaje('mensaje', '✅ Jefe asignado', 'exito');
        e.target.reset();
    } catch (err) {
        mostrarMensaje('mensaje', err.message || 'Error al asignar jefe', 'error');
    }
});

document.getElementById('formQuitarVotante').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
        const cedula = document.getElementById('cedulaV').value.trim();

        await fetchAPI('/Admin/quitar-votante', {
            method: 'POST',
            body: JSON.stringify({ cedula })
        });

        mostrarMensaje('mensaje', '🗑️ Ubicación quitada al votante', 'exito');
        e.target.reset();
    } catch (err) {
        mostrarMensaje('mensaje', err.message || 'Error al quitar votante', 'error');
    }
});

document.getElementById('formQuitarJefe').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
        const cedula = document.getElementById('cedulaJ').value.trim();

        await fetchAPI('/Admin/quitar-jefe', {
            method: 'POST',
            body: JSON.stringify({ cedula })
        });

        mostrarMensaje('mensaje', '🗑️ Ubicación quitada al jefe', 'exito');
        e.target.reset();
    } catch (err) {
        mostrarMensaje('mensaje', err.message || 'Error al quitar jefe', 'error');
    }
});
