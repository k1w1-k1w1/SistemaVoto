document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const btn = document.getElementById('btnLogin');
    btn.disabled = true;
    const originalText = btn.textContent;
    btn.textContent = "Ingresando...";

    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    try {
        const data = await fetchAPI('/Usuarios/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });

        // Guardar sesión
        guardarSesion(data.token, data.usuario);

        mostrarMensaje('mensaje', '✅ Bienvenido, redirigiendo...', 'exito');

        setTimeout(() => {
            switch (data.usuario.rol) {
                case 'Votante':
                    window.location.href = '/pages/votante/dashboard.html';
                    break;
                case 'JefeDeJunta':
                    window.location.href = '/pages/jefe/dashboard.html';
                    break;
                case 'Administrador':
                    window.location.href = '/pages/admin/dashboard.html';
                    break;
                default:
                    mostrarMensaje('mensaje', 'Rol no reconocido', 'error');
            }
        }, 1200);

    } catch (error) {
        mostrarMensaje('mensaje', error.message || 'Error al iniciar sesión', 'error');
        btn.disabled = false;
        btn.textContent = originalText;
    }
});
