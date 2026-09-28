document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("formInscripcion");
    const paisSelect = document.getElementById("pais");
    const carreraSelect = document.getElementById("carrera");
    const informacionAcademicaContainer = document.getElementById("informacionAcademica");
    const submitButton = document.getElementById("enviar");
    const message = document.getElementById("mensaje");
    const formularioStatus = document.getElementById("estadoFormulario");
    let formularioDisponible = false;

    function actualizarEstadoEnvio() {
        submitButton.disabled =
            paisSelect.disabled ||
            carreraSelect.disabled ||
            !formularioDisponible ||
            paisSelect.value === "" ||
            carreraSelect.value === "" ||
            informacionAcademicaContainer.querySelectorAll("input:checked").length === 0;
    }

    async function getJson(url, fallbackMessage) {
        const response = await fetch(url);
        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.message || error.detail || fallbackMessage);
        }

        return response.json();
    }

    async function cargarPaises() {
        paisSelect.disabled = true;
        paisSelect.replaceChildren(new Option("Cargando países...", ""));
        actualizarEstadoEnvio();

        try {
            const paises = await getJson("/api/paises", "No se pudo recuperar el listado de países.");
            paisSelect.replaceChildren(new Option(
                paises.length ? "Seleccioná un país." : "No hay países cargados.",
                ""
            ));
            for (const pais of paises) {
                paisSelect.add(new Option(pais.nombre, pais.id));
            }

            paisSelect.disabled = paises.length === 0;
            message.textContent = paises.length
                ? ""
                : "El administrador debe cargar al menos un país antes del alta.";
        } catch (error) {
            paisSelect.replaceChildren(new Option("No se pudo cargar el listado.", ""));
            paisSelect.disabled = true;
            message.textContent = error.message;
        }

        actualizarEstadoEnvio();
    }

    async function cargarCarreras() {
        carreraSelect.disabled = true;
        carreraSelect.replaceChildren(new Option("Cargando carreras...", ""));
        actualizarEstadoEnvio();

        try {
            const carreras = await getJson("/api/carreras", "No se pudo recuperar el listado de carreras.");
            carreraSelect.replaceChildren(new Option(
                carreras.length ? "Seleccioná una carrera." : "No hay carreras cargadas.",
                ""
            ));
            for (const carrera of carreras) {
                carreraSelect.add(new Option(`${carrera.nombre} — ${carrera.turno}`, carrera.id));
            }

            carreraSelect.disabled = carreras.length === 0;
            if (carreras.length === 0) {
                message.textContent = "No hay carreras disponibles para inscribirse.";
            }
        } catch (error) {
            carreraSelect.replaceChildren(new Option("No se pudo cargar el listado.", ""));
            carreraSelect.disabled = true;
            message.textContent = error.message;
        }

        actualizarEstadoEnvio();
    }

    async function cargarInformacionAcademica() {
        informacionAcademicaContainer.replaceChildren(
            document.createTextNode("Cargando opciones académicas...")
        );
        actualizarEstadoEnvio();

        try {
            const opciones = await getJson(
                "/api/informacion-academica",
                "No se pudo recuperar la información académica habilitada."
            );
            informacionAcademicaContainer.replaceChildren();
            if (opciones.length === 0) {
                informacionAcademicaContainer.textContent = "No hay opciones académicas habilitadas.";
                message.textContent = "El administrador debe habilitar al menos una opción académica antes del alta.";
                return;
            }

            for (const opcion of opciones) {
                const label = document.createElement("label");
                const checkbox = document.createElement("input");
                checkbox.type = "checkbox";
                checkbox.name = "informacionAcademicaIds";
                checkbox.value = opcion.id;
                checkbox.addEventListener("change", actualizarEstadoEnvio);
                label.append(checkbox, document.createTextNode(` ${opcion.descripcion}`));
                informacionAcademicaContainer.append(label);
            }
        } catch (error) {
            informacionAcademicaContainer.textContent = "No se pudieron cargar las opciones académicas.";
            message.textContent = error.message;
        }

        actualizarEstadoEnvio();
    }

    async function cargarDisponibilidad() {
        formularioDisponible = false;
        formularioStatus.textContent = "Verificando vigencia del formulario...";
        actualizarEstadoEnvio();

        try {
            const disponibilidad = await getJson(
                "/api/formulario/disponibilidad",
                "No se pudo verificar la vigencia del formulario."
            );
            formularioDisponible = disponibilidad.disponible;
            formularioStatus.textContent = formularioDisponible
                ? `Inscripción habilitada para el año ${disponibilidad.habilitacion.año}.`
                : "La inscripción está cerrada en este momento.";
        } catch (error) {
            formularioStatus.textContent = error.message;
        }

        actualizarEstadoEnvio();
    }

    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (!form.reportValidity()) {
            return;
        }

        submitButton.disabled = true;
        message.textContent = "Guardando estudiante...";

        const formData = new FormData(form);
        const solicitud = {
            dni: Number(formData.get("dni")),
            apellidosYNombres: formData.get("apellidosynombres").trim(),
            fechaNacimiento: formData.get("fechaNac"),
            correo: formData.get("correo").trim(),
            domicilio: formData.get("domicilio").trim(),
            telefono: formData.get("telefono").trim(),
            fechaEgresoSecundario: formData.get("fechaEgre") || null,
            tituloSecundario: formData.get("tituloSec").trim(),
            paisId: Number(formData.get("paisId")),
            carreraId: Number(formData.get("carreraId")),
            informacionAcademicaIds: formData.getAll("informacionAcademicaIds").map(Number)
        };

        try {
            const response = await fetch("/api/estudiantes", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(solicitud)
            });
            const result = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(result.message || result.detail || result.title || "No se pudo guardar el estudiante.");
            }

            form.reset();
            await Promise.all([cargarPaises(), cargarCarreras(), cargarInformacionAcademica()]);
            message.textContent = `Inscripción registrada. ID de estudiante: ${result.idEstudiante}.`;
        } catch (error) {
            message.textContent = error.message;
        } finally {
            actualizarEstadoEnvio();
        }
    });

    window.addEventListener("focus", () => {
        cargarPaises();
        cargarCarreras();
        cargarInformacionAcademica();
        cargarDisponibilidad();
    });

    void Promise.all([
        cargarPaises(),
        cargarCarreras(),
        cargarInformacionAcademica(),
        cargarDisponibilidad()
    ]);
});
