using CarWorkshopManager.Data;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations
{
    public class VatRateService : IVatRateService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<VatRateService> _logger;

        public VatRateService(
            ApplicationDbContext db,
            ILogger<VatRateService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<SelectList> GetSelectVatRatesListAsync()
        {
            _logger.LogInformation("GetSelectVatRatesListAsync called");
            var list = await _db.VatRates
                .OrderBy(v => v.Rate)
                .AsNoTracking()
                .ToListAsync();

            var items = list.Select(v => new { v.Id, Display = $"{v.Rate:P0}" });
            _logger.LogInformation("GetSelectVatRatesListAsync: returning {Count} rates", list.Count);
            return new SelectList(items, "Id", "Display");
        }
    }
}
