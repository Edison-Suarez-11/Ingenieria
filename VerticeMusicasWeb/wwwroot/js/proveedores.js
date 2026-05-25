(function () {
    const form = document.getElementById('form-proveedor');
    if (!form) return;

    const alertBox = document.getElementById('proveedor-alert');
    const tabla = document.getElementById('tabla-proveedores');
    const sinProveedores = document.getElementById('sin-proveedores');
    const tableWrap = document.getElementById('proveedores-table-wrap');
    const tituloForm = document.getElementById('form-proveedor-titulo');
    const formCard = document.getElementById('proveedores-form-card');
    const btnGuardar = document.getElementById('btn-guardar-proveedor');
    const btnCancelar = document.getElementById('btn-cancelar-edicion');
    const btnNuevo = document.getElementById('btn-nuevo-proveedor');
    const inputId = document.getElementById('IdProveedor');
    const inputNombre = document.getElementById('Nombre');
    const inputContacto = document.getElementById('Contacto');
    const countVisible = document.getElementById('proveedor-count-visible');
    const countTotal = document.getElementById('proveedor-count-total');
    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';

    const urls = {
        crear: '/Proveedores/Create',
        editar: '/Proveedores/Edit'
    };

    function actualizarContadores() {
        const rows = tabla ? tabla.querySelectorAll('tr[data-id]').length : 0;
        if (countVisible) countVisible.textContent = String(rows);
    }

    function marcarFilaEditando(id) {
        tabla?.querySelectorAll('tr[data-id]').forEach(tr => {
            tr.classList.toggle('is-editing-row', tr.dataset.id === String(id));
        });
    }

    function mostrarAlerta(mensaje, tipo) {
        alertBox.textContent = mensaje;
        alertBox.className = `alert alert-modern alert-${tipo}`;
        alertBox.classList.remove('d-none');
        alertBox.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function limpiarErrores() {
        form.querySelectorAll('[data-error-for]').forEach(el => {
            el.textContent = '';
        });
    }

    function mostrarErrores(errores) {
        limpiarErrores();
        if (!errores) return;

        Object.keys(errores).forEach(campo => {
            const span = form.querySelector(`[data-error-for="${campo}"]`);
            if (span && errores[campo]?.length) {
                span.textContent = errores[campo][0];
            }
        });
    }

    function modoRegistro() {
        tituloForm.innerHTML = '<span class="dot"></span> Registrar proveedor';
        btnGuardar.textContent = 'Guardar';
        btnCancelar.classList.add('d-none');
        formCard?.classList.remove('is-editing');
        inputId.value = '0';
        form.reset();
        limpiarErrores();
        marcarFilaEditando(-1);
    }

    function modoEdicion(proveedor) {
        const id = proveedor.idProveedor ?? proveedor.IdProveedor;
        tituloForm.innerHTML = '<span class="dot"></span> Editar proveedor';
        btnGuardar.textContent = 'Actualizar';
        btnCancelar.classList.remove('d-none');
        formCard?.classList.add('is-editing');
        inputId.value = id;
        inputNombre.value = proveedor.nombre ?? proveedor.Nombre ?? '';
        inputContacto.value = proveedor.contacto ?? proveedor.Contacto ?? '';
        limpiarErrores();
        marcarFilaEditando(id);
        formCard?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    function inicialProveedor(nombre) {
        const n = (nombre || '').trim();
        return n ? n[0].toUpperCase() : '?';
    }

    function crearFila(proveedor) {
        const id = proveedor.idProveedor ?? proveedor.IdProveedor;
        const nombre = proveedor.nombre ?? proveedor.Nombre ?? '';
        const contacto = proveedor.contacto ?? proveedor.Contacto ?? '';
        const inicial = inicialProveedor(nombre);
        const tr = document.createElement('tr');
        tr.dataset.id = id;
        tr.innerHTML = `
            <td>
                <div class="proveedores-cell-nombre">
                    <span class="proveedores-avatar">${inicial}</span>
                    <div>
                        <div class="name col-nombre"></div>
                        <div class="id-tag">ID ${id}</div>
                    </div>
                </div>
            </td>
            <td class="col-contacto proveedores-contact"></td>
            <td class="text-end">
                <button type="button" class="btn btn-sm btn-modern-secondary btn-editar-proveedor"
                        data-id="${id}"
                        data-nombre="${escapeAttr(nombre)}"
                        data-contacto="${escapeAttr(contacto)}">
                    Editar
                </button>
            </td>`;
        tr.querySelector('.col-nombre').textContent = nombre;
        tr.querySelector('.col-contacto').textContent = contacto;
        return tr;
    }

    function escapeAttr(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;');
    }

    function actualizarFila(proveedor) {
        const id = proveedor.idProveedor ?? proveedor.IdProveedor;
        const fila = tabla?.querySelector(`tr[data-id="${id}"]`);
        if (!fila) return;

        const nombre = proveedor.nombre ?? proveedor.Nombre ?? '';
        const contacto = proveedor.contacto ?? proveedor.Contacto ?? '';

        fila.querySelector('.col-nombre').textContent = nombre;
        fila.querySelector('.col-contacto').textContent = contacto;
        const avatar = fila.querySelector('.proveedores-avatar');
        if (avatar) avatar.textContent = inicialProveedor(nombre);

        const btn = fila.querySelector('.btn-editar-proveedor');
        btn.dataset.nombre = nombre;
        btn.dataset.contacto = contacto;
    }

    function agregarFila(proveedor) {
        if (!tabla) return;
        const fila = crearFila(proveedor);
        tabla.prepend(fila);
        sinProveedores?.classList.add('d-none');
        tableWrap?.classList.remove('d-none');
        if (countTotal) {
            const n = parseInt(countTotal.textContent, 10) || 0;
            countTotal.textContent = String(n + 1);
        }
        actualizarContadores();
    }

    async function enviarFormulario(url, datos) {
        const body = new URLSearchParams();
        body.append('__RequestVerificationToken', token);
        body.append('IdProveedor', datos.idProveedor);
        body.append('Nombre', datos.nombre);
        body.append('Contacto', datos.contacto);

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body
        });

        return response.json();
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        limpiarErrores();

        const id = parseInt(inputId.value, 10) || 0;
        const datos = {
            idProveedor: id,
            nombre: inputNombre.value.trim(),
            contacto: inputContacto.value.trim()
        };

        const url = id > 0 ? `${urls.editar}/${id}` : urls.crear;
        const resultado = await enviarFormulario(url, datos);

        if (!resultado.exito) {
            mostrarAlerta(resultado.mensaje || 'No se pudo guardar el proveedor.', 'danger');
            mostrarErrores(resultado.errores);
            return;
        }

        mostrarAlerta(resultado.mensaje, 'success');

        if (id > 0) {
            actualizarFila(resultado.proveedor);
        } else {
            agregarFila(resultado.proveedor);
        }

        modoRegistro();
    });

    tabla?.addEventListener('click', (e) => {
        const btn = e.target.closest('.btn-editar-proveedor');
        if (!btn) return;

        modoEdicion({
            IdProveedor: btn.dataset.id,
            Nombre: btn.dataset.nombre,
            Contacto: btn.dataset.contacto
        });
    });

    btnCancelar?.addEventListener('click', modoRegistro);
    btnNuevo?.addEventListener('click', () => {
        modoRegistro();
        inputNombre.focus();
        formCard?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });

    actualizarContadores();
})();
