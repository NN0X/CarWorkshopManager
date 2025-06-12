using System;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using CarWorkshopManager.ViewModels.ServiceOrder;

namespace CarWorkshopManager.Documents
{
    public class MonthlyRepairSummaryDocument : IDocument
    {
        private readonly MonthlyRepairSummaryReportViewModel _model;

        public MonthlyRepairSummaryDocument(MonthlyRepairSummaryReportViewModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text($"Podsumowanie napraw – {_model.Month:yyyy-MM}")
                    .FontSize(18).Bold();

                page.Content().Table(tbl =>
                {
                    tbl.ColumnsDefinition(cd =>
                    {
                        cd.RelativeColumn(3);
                        cd.RelativeColumn(2);
                        cd.RelativeColumn(1);
                        cd.RelativeColumn(1);
                        cd.RelativeColumn(1);
                        cd.RelativeColumn(1);
                    });

                    tbl.Header(header =>
                    {
                        header.Cell().Text("Klient").Bold();
                        header.Cell().Text("Rejestracja").Bold();
                        header.Cell().Text("Zleceń").Bold();
                        header.Cell().Text("Netto").Bold();
                        header.Cell().Text("VAT").Bold();
                        header.Cell().Text("Brutto").Bold();
                    });

                    foreach (var i in _model.Items)
                    {
                        tbl.Cell().Text(i.CustomerName);
                        tbl.Cell().Text(i.RegistrationNumber);
                        tbl.Cell().Text(i.OrdersCount.ToString());
                        tbl.Cell().Text(i.TotalCostNet.ToString("0.00"));
                        tbl.Cell().Text(i.TotalVat.ToString("0.00"));
                        tbl.Cell().Text(i.TotalCostGross.ToString("0.00"));
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Wygenerowano: ").FontSize(9);
                    x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).FontSize(9);
                });
            });
        }
    }
}
