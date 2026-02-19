// Funciones de autenticación

function guardarSesion(token, usuario) {
    localStorage.setItem('token', token);
    localStorage.setItem('usuario', JSON.stringify(usuario));
}

function obtenerUsuario() {
    const usuario = localStorage.getItem('usuario');
    return usuario ? JSON.parse(usuario) : null;
}

function obtenerToken() {
    return localStorage.getItem('token');
}

function cerrarSesion() {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
    window.location.href = '/index.html';
}

function verificarAutenticacion() {
    const token = obtenerToken();
    if (!token) {
        window.location.href = '/index.html';
        return false;
    }
    return true;
}

function verificarRol(rolesPermitidos) {
    const usuario = obtenerUsuario();
    if (!usuario || !rolesPermitidos.includes(usuario.rol)) {
        alert('No tienes permisos para acceder a esta página');
        window.location.href = '/index.html';
        return false;
    }
    return true;
}