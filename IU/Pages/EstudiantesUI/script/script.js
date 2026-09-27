document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("formInscripcion");
    const paisSelect = document.getElementById("pais");
    const submitButton = document.getElementById("enviar");
    const message = document.getElementById("mensaje");

    async function cargarPaises() {
        paisSelect.disabled = true;
        submitButton.disabled = true;
        paisSelect.replaceChildren(new Option("Cargando países...", ""));

        try {
            const response = await fetch("/api/paises");
            if (!response.ok) {
                throw new Error("No se pudo recuperar el listado de países.");
            }

            const paises = await response.json();
            paisSelect.replaceChildren(new Option(
                paises.length ? "Seleccioná un país." : "No hay países cargados.",
                ""
            ));

            for (const pais of paises) {
                paisSelect.add(new Option(pais.nombre, pais.id));
            }

            paisSelect.disabled = paises.length === 0;
            submitButton.disabled = paises.length === 0;
            message.textContent = paises.length
                ? ""
                : "El administrador debe cargar al menos un país antes del alta.";
        } catch (error) {
            paisSelect.replaceChildren(new Option("No se pudo cargar el listado.", ""));
            message.textContent = error.message;
        }
    }

    form.addEventListener("submit", async (event) => {
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
            paisId: Number(formData.get("paisId"))
        };

        try {
            const response = await fetch("/api/estudiantes", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(solicitud)
            });
            const result = await response.json();
            if (!response.ok) {
                throw new Error(result.message || result.title || "No se pudo guardar el estudiante.");
            }

            message.textContent = `Estudiante guardado. ID: ${result.idEstudiante}.`;
            form.reset();
            await cargarPaises();
        } catch (error) {
            message.textContent = error.message;
            submitButton.disabled = paisSelect.options.length < 2;
        }
    });

    window.addEventListener("focus", cargarPaises);
    cargarPaises();
});
