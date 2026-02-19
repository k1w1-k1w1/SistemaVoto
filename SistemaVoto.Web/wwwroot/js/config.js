// Configuración global de la API
const API_URL = 'https://sistemavoto.onrender.com/api';

// Helper para hacer peticiones a la API (a prueba de 204 / body vacío)
async function fetchAPI(endpoint, options = {}) {
    const token = localStorage.getItem('token');

    const headers = {
        ...(options.headers || {}),
    };

    // Solo pon Content-Type si NO estás mandando FormData
    const isFormData = options.body instanceof FormData;
    if (!isFormData) {
        headers['Content-Type'] = headers['Content-Type'] || 'application/json';
    }

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_URL}${endpoint}`, {
        ...options,
        headers
    });

    if (response.status === 204) return null;

    //  lee el body como texto primero
    const text = await response.text();

    //  intenta convertir a JSON solo si hay contenido
    let data = null;
    if (text) {
        try {
            data = JSON.parse(text);
        } catch (e) {
            // si vino HTML u otra cosa, lo dejamos como texto
            data = { raw: text };
        }
    }

    if (!response.ok) {
        const msg =
            data?.mensaje ||
            data?.title ||
            (typeof data?.raw === 'string' ? data.raw : null) ||
            `Error en la petición (${response.status})`;

        const err = new Error(msg);
        err.status = response.status;
        err.data = data;
        throw err;
    }

    //  si fue 204 o no vino body, devuelve null
    return data;
}

// Mostrar mensajes
function mostrarMensaje(elementoId, mensaje, tipo = 'error') {
    const elemento = document.getElementById(elementoId);
    if (elemento) {
        elemento.textContent = mensaje;
        elemento.className = `mensaje ${tipo}`;
        elemento.classList.remove('oculto');

        setTimeout(() => {
            elemento.classList.add('oculto');
        }, 5000);
    }
}
