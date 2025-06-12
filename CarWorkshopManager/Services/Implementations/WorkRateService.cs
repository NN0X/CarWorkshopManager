using CarWorkshopManager.Data;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class WorkRateService : IWorkRateService
{
    private readonly ApplicationDbContext _db;

    public WorkRateService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SelectList> GetSelectWorkRatesAsync()
    {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var list = await _db.WorkRates
                .Where(w => w.ValidTo == null || w.ValidTo >= today)
                .OrderBy(w => w.HourRateNet)
                .Select(w => new
                {
                    w.Id,
                    Display = $"{w.Name} – {w.HourRateNet:0.00} zł/h"
                })
                .ToListAsync();

            return new SelectList(list, "Id", "Display");
        }
}