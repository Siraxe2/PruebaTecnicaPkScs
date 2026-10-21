using ClosedXML.Excel;
using PruebaTecnicaPkScs.Models;

namespace PruebaTecnicaPkScs.Services
{
    public class ExcelExportService
    {
        public byte[] ExportPokemon(
        IReadOnlyCollection<PokemonApiItem> pokemon,
        string? name,
        string? species,
        int page,
        int totalMatches)
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Pokemon");

            // Encabezados del archivo.
            worksheet.Cell("A1").Value = "ID";
            worksheet.Cell("B1").Value = "Nombre";
            worksheet.Cell("C1").Value = "URL de imagen";

            // Estilo de los encabezados.
            var header = worksheet.Range("A1:C1");
            header.Style.Font.Bold = true;

            int row = 2;

            foreach (var item in pokemon)
            {
                worksheet.Cell(row, 1).Value = item.Id;
                worksheet.Cell(row, 2).Value = item.Name;
                worksheet.Cell(row, 3).Value = item.ImageUrl;

                row++;
            }

            // Ajustar el ancho de las columnas al contenido.
            worksheet.Columns("A:C").AdjustToContents();
            worksheet.Columns("E:F").AdjustToContents();

            // Generar el archivo en memoria.
            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}
