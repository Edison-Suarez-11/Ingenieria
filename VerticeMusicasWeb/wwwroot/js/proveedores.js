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
    const inputNit = document.getElementById('Nit');
    const inputPersonaContacto = document.getElementById('PersonaContacto');
    const inputCelular = document.getElementById('Celular');
    const inputCorreo = document.getElementById('CorreoElectronico');
    const inputTelefono = document.getElementById('TelefonoFijo');
    const inputCiudad = document.getElementById('Ciudad');
    const inputDireccion = document.getElementById('Direccion');
    const countVisible = document.getElementById('proveedor-count-visible');
    const countTotal = document.getElementById('proveedor-count-total');
    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';

    const urls = {
        crear: '/Proveedores/Create',
        editar: '/Proveedores/Edit'
    };

    function val(proveedor, camel, pascal) {
        return proveedor[camel] ?? proveedor[pascal] ?? '';
    }

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
        const id = val(proveedor, 'idProveedor', 'IdProveedor');
        tituloForm.innerHTML = '<span class="dot"></span> Editar proveedor';
        btnGuardar.textContent = 'Actualizar';
        btnCancelar.classList.remove('d-none');
        formCard?.classList.add('is-editing');
        inputId.value = id;
        inputNombre.value = val(proveedor, 'nombre', 'Nombre');
        inputNit.value = val(proveedor, 'nit', 'Nit');
        inputPersonaContacto.value = val(proveedor, 'personaContacto', 'PersonaContacto');
        inputCelular.value = val(proveedor, 'celular', 'Celular');
        inputCorreo.value = val(proveedor, 'correoElectronico', 'CorreoElectronico');
        inputTelefono.value = val(proveedor, 'telefonoFijo', 'TelefonoFijo');
        inputCiudad.value = val(proveedor, 'ciudad', 'Ciudad');
        inputDireccion.value = val(proveedor, 'direccion', 'Direccion');
        limpiarErrores();
        marcarFilaEditando(id);
        formCard?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    function inicialProveedor(nombre) {
        const n = (nombre || '').trim();
        return n ? n[0].toUpperCase() : '?';
    }

    function resumenContacto(proveedor) {
        const partes = [];
        const celular = val(proveedor, 'celular', 'Celular');
        const correo = val(proveedor, 'correoElectronico', 'CorreoElectronico');
        const contacto = val(proveedor, 'contacto', 'Contacto');
        if (celular) partes.push(celular);
        if (correo) partes.push(correo);
        if (partes.length) return partes.join(' · ');
        return contacto || '—';
    }

    function escapeAttr(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;');
    }

    function crearFila(proveedor) {
        const id = val(proveedor, 'idProveedor', 'IdProveedor');
        const nombre = val(proveedor, 'nombre', 'Nombre');
        const nit = val(proveedor, 'nit', 'Nit');
        const persona = val(proveedor, 'personaContacto', 'PersonaContacto');
        const celular = val(proveedor, 'celular', 'Celular');
        const correo = val(proveedor, 'correoElectronico', 'CorreoElectronico');
        const telefono = val(proveedor, 'telefonoFijo', 'TelefonoFijo');
        const ciudad = val(proveedor, 'ciudad', 'Ciudad');
        const direccion = val(proveedor, 'direccion', 'Direccion');
        const inicial = inicialProveedor(nombre);
        const resumen = resumenContacto(proveedor);
        const tr = document.createElement('tr');
        tr.dataset.id = id;
        tr.innerHTML = `
            <td>
                <div class="proveedores-cell-nombre">
                    <span class="proveedores-avatar">${inicial}</span>
                    <div>
                        <div class="name col-nombre"></div>
                        <div class="id-tag"></div>
                    </div>
                </div>
            </td>
            <td class="col-contacto proveedores-contact">
                <div class="col-persona"></div>
                <div class="col-resumen"></div>
            </td>
            <td class="col-ciudad"></td>
            <td class="col-direccion proveedores-contact"></td>
            <td class="text-end">
                <button type="button" class="btn btn-sm btn-modern-secondary btn-editar-proveedor"
                        data-id="${id}"
                        data-nombre="${escapeAttr(nombre)}"
                        data-nit="${escapeAttr(nit)}"
                        data-persona-contacto="${escapeAttr(persona)}"
                        data-celular="${escapeAttr(celular)}"
                        data-correo="${escapeAttr(correo)}"
                        data-telefono="${escapeAttr(telefono)}"
                        data-ciudad="${escapeAttr(ciudad)}"
                        data-direccion="${escapeAttr(direccion)}">
                    Editar
                </button>
            </td>`;
        tr.querySelector('.col-nombre').textContent = nombre;
        tr.querySelector('.id-tag').textContent = nit ? `NIT ${nit}` : `ID ${id}`;
        const personaEl = tr.querySelector('.col-persona');
        if (persona) {
            personaEl.textContent = persona;
        } else {
            personaEl.remove();
        }
        tr.querySelector('.col-resumen').textContent = resumen;
        tr.querySelector('.col-ciudad').textContent = ciudad || '—';
        tr.querySelector('.col-direccion').textContent = direccion || '—';
        return tr;
    }

    function actualizarFila(proveedor) {
        const id = val(proveedor, 'idProveedor', 'IdProveedor');
        const fila = tabla?.querySelector(`tr[data-id="${id}"]`);
        if (!fila) return;

        const nueva = crearFila(proveedor);
        fila.replaceWith(nueva);
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

    function leerFormulario() {
        return {
            idProveedor: parseInt(inputId.value, 10) || 0,
            nombre: inputNombre.value.trim(),
            nit: inputNit.value.trim(),
            personaContacto: inputPersonaContacto.value.trim(),
            celular: inputCelular.value.trim(),
            correoElectronico: inputCorreo.value.trim(),
            telefonoFijo: inputTelefono.value.trim(),
            ciudad: inputCiudad.value.trim(),
            direccion: inputDireccion.value.trim()
        };
    }

    async function enviarFormulario(url, datos) {
        const body = new URLSearchParams();
        body.append('__RequestVerificationToken', token);
        body.append('IdProveedor', datos.idProveedor);
        body.append('Nombre', datos.nombre);
        body.append('Nit', datos.nit);
        body.append('PersonaContacto', datos.personaContacto);
        body.append('Celular', datos.celular);
        body.append('CorreoElectronico', datos.correoElectronico);
        body.append('TelefonoFijo', datos.telefonoFijo);
        body.append('Ciudad', datos.ciudad);
        body.append('Direccion', datos.direccion);

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

        const datos = leerFormulario();
        const url = datos.idProveedor > 0 ? `${urls.editar}/${datos.idProveedor}` : urls.crear;
        const resultado = await enviarFormulario(url, datos);

        if (!resultado.exito) {
            mostrarAlerta(resultado.mensaje || 'No se pudo guardar el proveedor.', 'danger');
            mostrarErrores(resultado.errores);
            return;
        }

        mostrarAlerta(resultado.mensaje, 'success');

        if (datos.idProveedor > 0) {
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
            Nit: btn.dataset.nit,
            PersonaContacto: btn.dataset.personaContacto,
            Celular: btn.dataset.celular,
            CorreoElectronico: btn.dataset.correo,
            TelefonoFijo: btn.dataset.telefono,
            Ciudad: btn.dataset.ciudad,
            Direccion: btn.dataset.direccion
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
