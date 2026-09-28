using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using Administracion.Domain;

namespace Api.Exports;

public static class EstudianteExcelExporter
{
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static readonly string[] Headers =
    [
        "Correo electrónico",
        "Carrera inscripta",
        "Nombre y apellido",
        "DNI",
        "Teléfono",
        "Fecha de nacimiento",
        "Edad al 30/06",
        "Dirección",
        "Información académica",
        "Título secundario",
        "Fecha de egreso"
    ];

    public static byte[] Crear(IReadOnlyList<EstudianteExportacion> estudiantes)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddXml(archive, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);
            AddXml(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            AddXml(archive, "xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Estudiantes" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            AddXml(archive, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);
            AddXml(archive, "xl/styles.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <numFmts count="1"><numFmt numFmtId="164" formatCode="dd/mm/yyyy"/></numFmts>
                  <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
                  <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>
                  <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
                  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
                  <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="164" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/></cellXfs>
                  <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
                </styleSheet>
                """);
            AddWorksheet(archive, estudiantes);
        }

        return output.ToArray();
    }

    private static void AddWorksheet(ZipArchive archive, IReadOnlyList<EstudianteExportacion> estudiantes)
    {
        var entry = archive.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            CloseOutput = false
        });

        writer.WriteStartDocument(standalone: true);
        writer.WriteStartElement("worksheet", SpreadsheetNamespace);
        writer.WriteAttributeString("xmlns", "r", null, RelationshipNamespace);
        writer.WriteStartElement("sheetData", SpreadsheetNamespace);

        WriteRowStart(writer, 1);
        for (var index = 0; index < Headers.Length; index++)
        {
            WriteStringCell(writer, CellReference(index + 1, 1), Headers[index]);
        }
        writer.WriteEndElement();

        for (var rowIndex = 0; rowIndex < estudiantes.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 2;
            var estudiante = estudiantes[rowIndex];
            WriteRowStart(writer, rowNumber);
            WriteStringCell(writer, CellReference(1, rowNumber), estudiante.CorreoElectronico);
            WriteStringCell(writer, CellReference(2, rowNumber), estudiante.CarreraInscripta);
            WriteStringCell(writer, CellReference(3, rowNumber), estudiante.NombreApellido);
            WriteNumberCell(writer, CellReference(4, rowNumber), estudiante.Dni);
            WriteStringCell(writer, CellReference(5, rowNumber), estudiante.Telefono);
            WriteDateCell(writer, CellReference(6, rowNumber), estudiante.FechaNacimiento);
            WriteNumberCell(writer, CellReference(7, rowNumber), estudiante.EdadActual);
            WriteStringCell(writer, CellReference(8, rowNumber), estudiante.Direccion);
            WriteStringCell(writer, CellReference(9, rowNumber), estudiante.Posee);
            WriteStringCell(writer, CellReference(10, rowNumber), estudiante.TituloSecundario);
            if (estudiante.AñoEgreso is { } fechaEgreso)
            {
                WriteDateCell(writer, CellReference(11, rowNumber), fechaEgreso);
            }
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteRowStart(XmlWriter writer, int rowNumber)
    {
        writer.WriteStartElement("row", SpreadsheetNamespace);
        writer.WriteAttributeString("r", rowNumber.ToString(CultureInfo.InvariantCulture));
    }

    private static void WriteStringCell(XmlWriter writer, string reference, string? value)
    {
        writer.WriteStartElement("c", SpreadsheetNamespace);
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("t", "inlineStr");
        writer.WriteStartElement("is", SpreadsheetNamespace);
        writer.WriteStartElement("t", SpreadsheetNamespace);
        writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
        writer.WriteString(value ?? string.Empty);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteNumberCell(XmlWriter writer, string reference, int value)
    {
        writer.WriteStartElement("c", SpreadsheetNamespace);
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("t", "n");
        writer.WriteElementString("v", SpreadsheetNamespace, value.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
    }

    private static void WriteDateCell(XmlWriter writer, string reference, DateTime value)
    {
        writer.WriteStartElement("c", SpreadsheetNamespace);
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("s", "1");
        writer.WriteAttributeString("t", "n");
        writer.WriteElementString("v", SpreadsheetNamespace, value.Date.ToOADate().ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
    }

    private static string CellReference(int column, int row)
    {
        var name = string.Empty;
        while (column > 0)
        {
            column--;
            name = (char)('A' + column % 26) + name;
            column /= 26;
        }

        return name + row.ToString(CultureInfo.InvariantCulture);
    }

    private static void AddXml(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }
}
