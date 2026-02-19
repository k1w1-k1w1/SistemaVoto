document.getElementById('registroForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const btn = document.getElementById('btnRegistrar');
    btn.disabled = true;
    const originalText = btn.textContent;
    btn.textContent = 'Registrando...';

    const datos = {
        nombre: document.getElementById('nombre').value.trim(),
        apellido: document.getElementById('apellido').value.trim(),
        cedula: document.getElementById('cedula').value.trim(),
        email: document.getElementById('email').value.trim(),
        telefono: document.getElementById('telefono').value.trim(),
        password: document.getElementById('password').value
    };

    // Validaciones rápidas (front)
    if (datos.cedula.length !== 10 || !/^\d{10}$/.test(datos.cedula)) {
        mostrarMensaje('mensaje', 'La cédula debe tener 10 dígitos.', 'error');
        btn.disabled = false;
        btn.textContent = originalText;
        return;
    }

    try {
        await fetchAPI('/Usuarios/registro', {
            method: 'POST',
            body: JSON.stringify(datos)
        });

        mostrarMensaje(
            'mensaje',
            '✅ Registro exitoso. Revisa tu correo para confirmar la cuenta.',
            'exito'
        );

        setTimeout(() => {
            window.location.href = '/index.html';
        }, 2200);

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al registrarse', 'error');
        btn.disabled = false;
        btn.textContent = originalText;
    }
});
