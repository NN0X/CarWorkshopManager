using System;
using System.Collections.Generic;
using System.IO;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CarWorkshopManager.Documents
{
    public class OpenOrderItemViewModel
    {
        public int ServiceOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class OpenOrdersReportViewModel
    {
        public DateTime GeneratedAt { get; set; }
        public List<OpenOrderItemViewModel> Items { get; set; } = new();
    }

    public class OpenOrdersReportDocument : IDocument
    {
        private readonly OpenOrdersReportViewModel _model;

        public OpenOrdersReportDocument(OpenOrdersReportViewModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);

                page.Header().Text($"Raport otwartych zleceń – {_model.GeneratedAt:yyyy-MM-dd HH:mm}")
                    .FontSize(18).SemiBold();

                page.Content().Table(tbl =>
                {
                    tbl.ColumnsDefinition(cd =>
                    {
                        cd.RelativeColumn(1); // LP
                        cd.RelativeColumn(2); // Numer
                        cd.RelativeColumn(2); // Otwarte od
                        cd.RelativeColumn(3); // Klient
                        cd.RelativeColumn(2); // Rejestracja
                        cd.RelativeColumn(2); // Status
                    });

                    tbl.Header(header =>
                    {
                        header.Cell().Text("LP").Bold();
                        header.Cell().Text("Numer").Bold();
                        header.Cell().Text("Otwarte od").Bold();
                        header.Cell().Text("Klient").Bold();
                        header.Cell().Text("Rej.").Bold();
                        header.Cell().Text("Status").Bold();
                    });

                    int lp = 1;
                    foreach (var item in _model.Items)
                    {
                        tbl.Cell().Text(lp++.ToString());
                        tbl.Cell().Text(item.OrderNumber);
                        tbl.Cell().Text(item.OpenedAt.ToString("yyyy-MM-dd"));
                        tbl.Cell().Text(item.CustomerName);
                        tbl.Cell().Text(item.RegistrationNumber);
                        tbl.Cell().Text(item.Status);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Wygenerowano: ").FontSize(9);
                    x.Span(_model.GeneratedAt.ToString("yyyy-MM-dd HH:mm")).FontSize(9);
                });
            });
        }

        public void GeneratePdf(Stream stream)
        {
            var document = Document.Create(container => Compose(container));
            document.GeneratePdf(stream);
        }
    }
}
