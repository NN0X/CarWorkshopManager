using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CarWorkshopManager.ViewModels.ServiceOrder;

namespace CarWorkshopManager.Documents
{
    public class RepairCostReportDocument : IDocument
    {
        private readonly RepairCostReportViewModel _model;

        public RepairCostReportDocument(RepairCostReportViewModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text("Raport kosztów napraw").FontSize(20).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Nr zlecenia");
                        header.Cell().Element(CellStyle).Text("Otwarte");
                        header.Cell().Element(CellStyle).Text("Robocizna");
                        header.Cell().Element(CellStyle).Text("Części");
                        header.Cell().Element(CellStyle).Text("Razem netto");
                        header.Cell().Element(CellStyle).Text("VAT");
                    });

                    foreach (var item in _model.Items)
                    {
                        table.Cell().Element(CellStyle).Text(item.OrderNumber);
                        table.Cell().Element(CellStyle).Text(item.OpenedAt.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(item.LaborNet.ToString("0.00"));
                        table.Cell().Element(CellStyle).Text(item.PartsNet.ToString("0.00"));
                        table.Cell().Element(CellStyle).Text(item.TotalNet.ToString("0.00"));
                        table.Cell().Element(CellStyle).Text(item.TotalVat.ToString("0.00"));
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    }
                });
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Wygenerowano: ").FontSize(9);
                    txt.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).FontSize(9).SemiBold();
                });
            });
        }
    }
}
