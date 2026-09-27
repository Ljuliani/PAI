document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("formPais");
    const message = document.getElementById("mensaje");
    const submitButton = document.getElementById("guardarPais");
    const paisList = document.getElementById("listaPaises");

    async function cargarPaises() {
        const response = await fetch("/api/paises");
        if (!response.ok) {
            throw new Error("No se pudo recuperar el listado de países.");
        }

        const paises = await response.json();
        paisList.replaceChildren();
        if (paises.length === 0) {
            const item = document.createElement("li");
            item.textContent = "Todavía no hay países cargados.";
            paisList.append(item);
            return;
        }

        for (const pais of paises) {
            const item = document.createElement("li");
            item.textContent = `${pais.id} — ${pais.nombre}`;
            paisList.append(item);
        }
    }

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (!form.reportValidity()) {
            return;
        }

        submitButton.disabled = true;
        message.textContent = "Guardando país...";
        const formData = new FormData(form);

        try {
            const response = await fetch("/api/paises", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    id: Number(formData.get("id")),
                    nombre: formData.get("nombre").trim()
                })
            });

            const result = await response.json();
            if (!response.ok) {
                throw new Error(result.message || result.title || "No se pudo guardar el país.");
            }

            message.textContent = "País guardado correctamente.";
            form.reset();
            await cargarPaises();
        } catch (error) {
            message.textContent = error.message;
        } finally {
            submitButton.disabled = false;
        }
    });

    cargarPaises().catch((error) => {
        message.textContent = error.message;
    });
});
