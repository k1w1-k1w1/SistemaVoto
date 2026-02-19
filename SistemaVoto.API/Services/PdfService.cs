using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SistemaVoto.API.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerarCertificadoPdf(
            string nombreCompleto,
            string cedula,
            string eleccionNombre,
            string eleccionTipo,
            string numeroConfirmacion,
            DateTime fechaEmisionUtc)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var fechaLocal = fechaEmisionUtc.ToLocalTime();

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CERTIFICADO DE VOTACIÓN")
                                .FontSize(18).Bold().FontColor("#C62828");
                            col.Item().Text("Sistema de Votación Electrónica")
                                .FontSize(11).FontColor("#666666");
                        });

                        row.ConstantItem(140).AlignRight().Text($"Confirmación:\n{numeroConfirmacion}")
                            .FontSize(10).SemiBold();
                    });

                    page.Content().PaddingTop(18).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().LineHorizontal(1).LineColor("#DDDDDD");

                        col.Item().Text("Se certifica que el ciudadano registró su participación en el proceso electoral.")
                            .FontSize(12);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            void Cell(string label, string value)
                            {
                                table.Cell().Element(e => e.Border(1).BorderColor("#EEEEEE").Padding(10).Background("#FAFAFA"))
                                    .Column(x =>
                                    {
                                        x.Item().Text(label.ToUpper()).FontSize(9).FontColor("#666666");
                                        x.Item().Text(value).Bold().FontSize(12);
                                    });
                            }

                            Cell("Votante", nombreCompleto);
                            Cell("Cédula", cedula);
                            Cell("Elección", eleccionNombre);
                            Cell("Tipo", eleccionTipo);
                            Cell("Número de confirmación", numeroConfirmacion);
                            Cell("Fecha de emisión", fechaLocal.ToString("yyyy-MM-dd HH:mm"));
                        });

                        col.Item().PaddingTop(10).Background("#FFF5F5").Border(1).BorderColor("#FFCDD2").Padding(12)
                            .Text("Este documento es válido como constancia de votación dentro del sistema.")
                            .FontColor("#A11616").SemiBold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Documento generado automáticamente • ");
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).FontColor("#666666");
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}
