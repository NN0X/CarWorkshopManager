using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarWorkshopManager.Documents;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF;

namespace CarWorkshopManager.Services.Implementations
{
    public class OpenOrdersReportBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OpenOrdersReportBackgroundService> _logger;
        private readonly int _intervalMinutes;
        private readonly string _adminEmail;

        public OpenOrdersReportBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OpenOrdersReportBackgroundService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _intervalMinutes = int.Parse(configuration["ReportSettings:IntervalMinutes"] ?? "1440");
            _adminEmail = configuration["ReportSettings:AdminEmail"]
                ?? throw new InvalidOperationException("ReportSettings:AdminEmail is not configured");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OpenOrdersReportBackgroundService started. Interval: {Minutes} minutes.", _intervalMinutes);

            await GenerateAndSendReport(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Waiting {Minutes} minutes until next report.", _intervalMinutes);
                    await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
                    await GenerateAndSendReport(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during report generation or sending. Retrying in 1 minute.");
                    try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
                    catch { }
                }
            }

            _logger.LogInformation("OpenOrdersReportBackgroundService is stopping.");
        }

        private async Task GenerateAndSendReport(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IServiceOrderService>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailWithAttachmentSender>();

            _logger.LogInformation("Generating open orders report...");

            var openOrders = await orderService.GetOpenServiceOrdersAsync();

            var reportModel = new OpenOrdersReportViewModel
            {
                GeneratedAt = DateTime.Now,
                Items = openOrders.Select(o => new OpenOrderItemViewModel
                {
                    ServiceOrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    OpenedAt = o.OpenedAt,
                    CustomerName = o.CustomerName,
                    RegistrationNumber = o.RegistrationNumber,
                    Status = o.StatusName
                }).ToList()
            };

            if (!reportModel.Items.Any())
            {
                _logger.LogInformation("No open orders found; skipping PDF generation and email.");
                return;
            }

            byte[] pdfBytes;
            try
            {
                using var ms = new MemoryStream();
                var document = new OpenOrdersReportDocument(reportModel);
                document.GeneratePdf(ms); // QuestPDF-extension
                pdfBytes = ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate PDF for open orders report.");
                return;
            }

            try
            {
                var timestamp = reportModel.GeneratedAt;
                string subject = $"Raport otwartych zleceń – {timestamp:yyyy-MM-dd HH:mm}";
                string htmlBody = $"<p>W załączeniu raport otwartych zleceń wygenerowany dnia {timestamp:yyyy-MM-dd HH:mm}.</p>";
                string filename = $"open_orders_{timestamp:yyyyMMdd_HHmm}.pdf";
                string mimeType = "application/pdf";

                await emailSender.SendEmailAsync(_adminEmail, subject, htmlBody, pdfBytes, filename, mimeType);
                _logger.LogInformation("Open orders report sent to {Email}", _adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email with open orders report.");
            }
        }
    }
}
