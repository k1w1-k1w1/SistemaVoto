// Verificar autenticación y rol
if (!verificarAutenticacion() || !verificarRol(['JefeDeJunta'])) {
    // Redirige automáticamente
}

const jefeId = localStorage.getItem('jefeDeJuntaId') || 1;

document.getElementById('reportarForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const tipo = document.getElementById('tipo').value;
    const cedula = document.getElementById('cedula').value;
    const descripcion = document.getElementById('descripcion').value;

    // Buscar votanteId si se proporcionó cédula
    let votanteId = null;
    if (cedula) {
        try {
            // Aquí podrías hacer una búsqueda del votante por cédula
            // Por ahora lo dejamos null si no es crítico
        } catch (error) {
            console.log('Votante no encontrado, continuando sin ID');
        }
    }

    try {
        const data = await fetchAPI('/JefeDeJunta/reportar-incidencia', {
            method: 'POST',
            body: JSON.stringify({ votanteId, tipo, descripcion })

        });

        mostrarMensaje('mensaje', '✅ Incidencia reportada exitosamente', 'exito');

        // Limpiar formulario
        setTimeout(() => {
            document.getElementById('reportarForm').reset();
            window.location.href = '/pages/Jefe/dashboard.html';
        }, 2000);

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al reportar incidencia', 'error');
    }
});