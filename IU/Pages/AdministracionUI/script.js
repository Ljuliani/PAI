document.addEventListener("DOMContentLoaded", async () => {
    const loginPanel = document.getElementById("loginPanel");
    const adminPanel = document.getElementById("adminPanel");
    const loginMessage = document.getElementById("loginMessage");
    const exportColumns = [
        ["correoElectronico", "Correo electrónico"],
        ["carreraInscripta", "Carrera inscripta"],
        ["nombreApellido", "Nombre y apellido"],
        ["dni", "DNI"],
        ["telefono", "Teléfono"],
        ["fechaNacimiento", "Fecha de nacimiento"],
        ["edadActual", "Edad al 30/06"],
        ["direccion", "Dirección"],
        ["posee", "Información académica"],
        ["tituloSecundario", "Título secundario"],
        ["añoEgreso", "Fecha de egreso"]
    ];

    async function request(url, options = {}) {
        const response = await fetch(url, {
            credentials: "same-origin",
            ...options,
            headers: {
                ...(options.body ? { "Content-Type": "application/json" } : {}),
                ...options.headers
            }
        });
        if (response.status === 401) {
            if (url === "/api/admin/login") {
                throw new Error("Usuario o contraseña incorrectos.");
            }
            showLogin();
            throw new Error("La sesión administrativa venció. Iniciá sesión nuevamente.");
        }
        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.message || error.detail || error.title || `Error HTTP ${response.status}.`);
        }
        if (response.status === 204) {
            return null;
        }
        return response.json();
    }

    function showLogin(message = "") {
        loginPanel.hidden = false;
        adminPanel.hidden = true;
        loginMessage.textContent = message;
    }

    function showAdmin(usuario) {
        loginPanel.hidden = true;
        adminPanel.hidden = false;
        document.getElementById("sessionUser").textContent = `Sesión: ${usuario || "Administrador"}`;
    }

    function cell(row, value) {
        const td = document.createElement("td");
        td.textContent = value ?? "";
        row.append(td);
        return td;
    }

    function actionButton(label, action, className = "secondary") {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
        button.textContent = label;
        button.addEventListener("click", action);
        return button;
    }

    async function cargarHabilitaciones() {
        const items = await request("/api/admin/habilitaciones");
        const tbody = document.getElementById("habilitacionesRows");
        tbody.replaceChildren();
        for (const item of items) {
            const row = document.createElement("tr");
            cell(row, item.año);
            cell(row, item.fechaInicio);
            cell(row, item.fechaCierre);
            cell(row, item.estado);
            const actions = cell(row, "");
            actions.append(
                actionButton("Editar", () => editarHabilitacion(item)),
                actionButton("Eliminar", () => eliminar("/api/admin/habilitaciones", item.id, cargarHabilitaciones), "danger")
            );
            tbody.append(row);
        }
    }

    function editarHabilitacion(item) {
        document.getElementById("habilitacionId").value = item.id;
        document.getElementById("habilitacionAnio").value = item.año;
        document.getElementById("habilitacionAnio").disabled = true;
        document.getElementById("habilitacionInicio").value = item.fechaInicio;
        document.getElementById("habilitacionCierre").value = item.fechaCierre;
        document.getElementById("cancelarHabilitacion").hidden = false;
    }

    function limpiarHabilitacion() {
        document.getElementById("habilitacionForm").reset();
        document.getElementById("habilitacionId").value = "";
        document.getElementById("habilitacionAnio").disabled = false;
        document.getElementById("cancelarHabilitacion").hidden = true;
    }

    async function cargarAcademica() {
        const items = await request("/api/admin/informacion-academica");
        const tbody = document.getElementById("academicaRows");
        tbody.replaceChildren();
        for (const item of items) {
            const row = document.createElement("tr");
            cell(row, item.descripcion);
            cell(row, item.fecha);
            cell(row, item.estado);
            const actions = cell(row, "");
            actions.append(
                actionButton("Editar", () => editarAcademica(item)),
                actionButton("Eliminar", () => eliminar(
                    "/api/admin/informacion-academica",
                    item.id,
                    cargarAcademica,
                    "Eliminar esta opción también la quitará de las relaciones académicas ya registradas. ¿Continuar?"
                ), "danger")
            );
            tbody.append(row);
        }
    }

    function editarAcademica(item) {
        document.getElementById("academicaId").value = item.id;
        document.getElementById("academicaDescripcion").value = item.descripcion;
        document.getElementById("academicaFecha").value = item.fecha;
        document.getElementById("academicaEstado").value = item.estado;
        document.getElementById("cancelarAcademica").hidden = false;
    }

    function limpiarAcademica() {
        document.getElementById("academicaForm").reset();
        document.getElementById("academicaId").value = "";
        document.getElementById("cancelarAcademica").hidden = true;
    }

    async function eliminar(path, id, reload, confirmation = "¿Querés eliminar este registro?") {
        if (!window.confirm(confirmation)) {
            return;
        }
        try {
            await request(`${path}/${id}`, { method: "DELETE" });
            await reload();
        } catch (error) {
            window.alert(error.message);
        }
    }

    async function cargarExportacion() {
        const rows = await request("/api/admin/exportar-estudiantes");
        const headers = document.getElementById("exportHeaders");
        const tbody = document.getElementById("exportRows");
        headers.replaceChildren();
        tbody.replaceChildren();
        for (const [, title] of exportColumns) {
            const th = document.createElement("th");
            th.textContent = title;
            headers.append(th);
        }
        for (const item of rows) {
            const tr = document.createElement("tr");
            for (const [key] of exportColumns) {
                cell(tr, item[key]);
            }
            tbody.append(tr);
        }
        document.getElementById("exportMessage").textContent = `${rows.length} estudiante(s) cargados.`;
    }

    async function cargarPanel() {
        const tasks = [
            [cargarHabilitaciones, "habilitacionesMessage"],
            [cargarAcademica, "academicaMessage"],
            [cargarExportacion, "exportMessage"]
        ];
        await Promise.all(tasks.map(async ([load, messageId]) => {
            try {
                await load();
            } catch (error) {
                document.getElementById(messageId).textContent = error.message;
            }
        }));
    }

    document.getElementById("loginForm").addEventListener("submit", async event => {
        event.preventDefault();
        loginMessage.textContent = "Validando acceso...";
        const data = new FormData(event.currentTarget);
        try {
            const result = await request("/api/admin/login", {
                method: "POST",
                body: JSON.stringify({
                    usuario: data.get("usuario"),
                    contraseña: data.get("contraseña")
                })
            });
            showAdmin(result.usuario);
            await cargarPanel();
        } catch (error) {
            showLogin(error.message);
        }
    });

    document.getElementById("logoutButton").addEventListener("click", async () => {
        try {
            await request("/api/admin/logout", { method: "POST" });
            document.getElementById("loginForm").reset();
            showLogin("Sesión cerrada.");
        } catch (error) {
            showLogin(error.message);
        }
    });

    document.querySelectorAll("nav [data-section]").forEach(button => {
        button.addEventListener("click", () => {
            document.querySelectorAll(".admin-section").forEach(section => {
                section.hidden = section.id !== button.dataset.section;
            });
            document.querySelectorAll("nav [data-section]").forEach(item => item.removeAttribute("aria-current"));
            button.setAttribute("aria-current", "page");
        });
    });

    document.getElementById("habilitacionForm").addEventListener("submit", async event => {
        event.preventDefault();
        const id = document.getElementById("habilitacionId").value;
        const body = {
            fechaInicio: document.getElementById("habilitacionInicio").value,
            fechaCierre: document.getElementById("habilitacionCierre").value
        };
        if (!id) {
            body.año = Number(document.getElementById("habilitacionAnio").value);
        }
        try {
            await request(id ? `/api/admin/habilitaciones/${id}` : "/api/admin/habilitaciones", {
                method: id ? "PUT" : "POST",
                body: JSON.stringify(body)
            });
            limpiarHabilitacion();
            document.getElementById("habilitacionesMessage").textContent = "Habilitación guardada.";
            await cargarHabilitaciones();
        } catch (error) {
            document.getElementById("habilitacionesMessage").textContent = error.message;
        }
    });

    document.getElementById("cancelarHabilitacion").addEventListener("click", limpiarHabilitacion);

    document.getElementById("academicaForm").addEventListener("submit", async event => {
        event.preventDefault();
        const id = document.getElementById("academicaId").value;
        const body = {
            descripcion: document.getElementById("academicaDescripcion").value.trim(),
            fecha: document.getElementById("academicaFecha").value,
            estado: document.getElementById("academicaEstado").value
        };
        try {
            await request(id ? `/api/admin/informacion-academica/${id}` : "/api/admin/informacion-academica", {
                method: id ? "PUT" : "POST",
                body: JSON.stringify(body)
            });
            limpiarAcademica();
            document.getElementById("academicaMessage").textContent = "Opción académica guardada.";
            await cargarAcademica();
        } catch (error) {
            document.getElementById("academicaMessage").textContent = error.message;
        }
    });

    document.getElementById("cancelarAcademica").addEventListener("click", limpiarAcademica);

    document.getElementById("exportButton").addEventListener("click", async () => {
        const message = document.getElementById("exportMessage");
        try {
            const response = await fetch("/api/admin/exportar-estudiantes.xlsx", {
                credentials: "same-origin"
            });
            if (response.status === 401) {
                showLogin();
                throw new Error("La sesión administrativa venció. Iniciá sesión nuevamente.");
            }
            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.message || error.detail || error.title || "No se pudo descargar el Excel.");
            }

            const url = URL.createObjectURL(await response.blob());
            const link = document.createElement("a");
            link.href = url;
            link.download = "Estudiantes.xlsx";
            link.click();
            URL.revokeObjectURL(url);
            message.textContent = "Archivo Excel descargado.";
        } catch (error) {
            message.textContent = error.message;
        }
    });

    try {
        const session = await request("/api/admin/sesion");
        showAdmin(session.usuario);
        await cargarPanel();
    } catch {
        showLogin();
    }
});
