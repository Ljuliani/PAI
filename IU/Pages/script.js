document.addEventListener("DOMContentLoaded", function () {
    const turnoSelect = document.getElementById("turno");
    const carreraSelect = document.getElementById("carrera");

    const carrerasPorTurno = {
        manana: ["Profesorado de Matemática"],
        tarde: ["Profesorado de Física"],
        vespertino: [
            "Profesorado de Economía",
            "Profesorado de Electromecánica",
            "Profesorado de Electrónica (Pendiente de Aprobación por la DGCyE)",
            "Profesorado de Matemática",
            "Tecnicatura Superior en Análisis de Sistemas",
            "Tecnicatura Superior en Bibliotecología",
            "Tecnicatura Superior en Bibliotecología de Instituciones Educativas (BIE) – Pendiente de aprobación por la DGCyE",
            "Tecnicatura Superior en Mantenimiento Industrial (Sólo 2° y 3° Año)"
        ]
    };

    turnoSelect.addEventListener("change", function () {
        const turno = turnoSelect.value;

        // Limpiamos el select y creamos la opción deshabilitada
        carreraSelect.innerHTML = "";
        const defaultOption = document.createElement("option");
        defaultOption.value = "";
        defaultOption.textContent = "Seleccioná una carrera.";
        defaultOption.disabled = true;
        defaultOption.selected = true;
        carreraSelect.appendChild(defaultOption);

        // Si hay un turno válido, agregamos las carreras correspondientes
        if (turno && carrerasPorTurno[turno]) {
            carrerasPorTurno[turno].forEach(carrera => {
                const option = document.createElement("option");
                option.value = carrera;
                option.textContent = carrera;
                carreraSelect.appendChild(option);
            });
        }
    });

    const formInscripcion = document.getElementById("formInscripcion");

    formInscripcion.addEventListener("submit", function (e) {
        e.preventDefault();

        if (formInscripcion.checkValidity()) {
            alert("✅ Formulario completado con éxito.\n\n📧 Se ha enviado una copia a su correo electrónico.\n\n🤝 ¡Muchas gracias por inscribirte!");
            formInscripcion.reset();
        } 
    });
});