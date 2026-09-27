CREATE OR ALTER PROCEDURE dbo.Estudiantes_Insert
    @Est_Nom_Ape NVARCHAR(200),
    @Est_DNI INT,
    @Est_Correo VARCHAR(50),
    @Est_Telefono VARCHAR(20),
    @Est_Fecha_Nac DATE,
    @Est_Direccion NVARCHAR(80),
    @Est_Titulo_Sec VARCHAR(50),
    @ID_Pais INT,
    @Est_Año_Egreso DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID_Pais IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Pais WHERE ID_PAIS = @ID_Pais)
    BEGIN
        THROW 50001, N'El código de país enviado no existe o es nulo.', 1;
    END;

    IF @Est_DNI IS NULL OR @Est_DNI <= 0
    BEGIN
        THROW 50002, N'El DNI debe ser un entero positivo.', 1;
    END;

    IF @Est_Telefono IS NULL
    BEGIN
        THROW 50003, N'El teléfono no puede ser nulo.', 1;
    END;

    IF LEN(@Est_Telefono) > 20
    BEGIN
        THROW 50004, N'El teléfono supera el máximo de 20 caracteres.', 1;
    END;

    IF @Est_Correo IS NULL
    BEGIN
        THROW 50005, N'El correo no puede ser nulo.', 1;
    END;

    IF LEN(@Est_Correo) > 50
    BEGIN
        THROW 50006, N'El correo supera el máximo de 50 caracteres.', 1;
    END;

    IF @Est_Nom_Ape IS NULL
    BEGIN
        THROW 50007, N'El nombre y apellido no pueden ser nulos.', 1;
    END;

    IF @Est_Fecha_Nac IS NULL
    BEGIN
        THROW 50008, N'La fecha de nacimiento no puede ser nula.', 1;
    END;

    IF @Est_Direccion IS NULL
    BEGIN
        THROW 50009, N'La dirección no puede ser nula.', 1;
    END;

    IF LEN(@Est_Direccion) > 80
    BEGIN
        THROW 50010, N'La dirección supera el máximo de 80 caracteres.', 1;
    END;

    IF @Est_Titulo_Sec IS NULL
    BEGIN
        THROW 50011, N'El título secundario no puede ser nulo.', 1;
    END;

    IF LEN(@Est_Titulo_Sec) > 50
    BEGIN
        THROW 50012, N'El título secundario supera el máximo de 50 caracteres.', 1;
    END;

    BEGIN TRY
        DECLARE @ID_Est INT;

        SELECT @ID_Est = ID_Est
        FROM dbo.Estudiantes
        WHERE Est_DNI = @Est_DNI
          AND Est_Correo = @Est_Correo;

        IF @ID_Est IS NULL
        BEGIN
            INSERT INTO dbo.Estudiantes (
                Est_Nom_Ape,
                Est_DNI,
                Est_Correo,
                Est_Telefono,
                Est_Fecha_Nac,
                Est_Direccion,
                Est_Titulo_Sec,
                Est_Año_Egreso,
                ID_Pais
            )
            VALUES (
                @Est_Nom_Ape,
                @Est_DNI,
                @Est_Correo,
                @Est_Telefono,
                @Est_Fecha_Nac,
                @Est_Direccion,
                @Est_Titulo_Sec,
                @Est_Año_Egreso,
                @ID_Pais
            );

            SET @ID_Est = CONVERT(INT, SCOPE_IDENTITY());
        END;

        SELECT @ID_Est AS ID_Est;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH;
END;
GO
