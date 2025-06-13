using CarWorkshopManager.Data;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarWorkshopManager.Services.Implementations
{
    public class WorkRateService : IWorkRateService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<WorkRateService> _logger;

        public WorkRateService(
            ApplicationDbContext db,
            ILogger<WorkRateService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<SelectList> GetSelectWorkRatesAsync()
        {
            _logger.LogInformation("GetSelectWorkRatesAsync called");
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var list = await _db.WorkRates
                .Where(w => w.ValidTo == null || w.ValidTo >= today)
                .OrderBy(w => w.HourRateNet)
                .Select(w => new { w.Id, Display = $"{w.Name} – {w.HourRateNet:0.00} zł/h" })
                .ToListAsync();

            _logger.LogInformation("GetSelectWorkRatesAsync: returning {Count} rates", list.Count);
            return new SelectList(list, "Id", "Display");
        }
    }
}
