using CarWorkshopManager.Data;
using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class VatRateService : IVatRateService
{
    private readonly ApplicationDbContext _db;

    public VatRateService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SelectList> GetSelectVatRatesListAsync()
    {
        var list = await _db.VatRates
            .OrderBy(v => v.Rate)
            .AsNoTracking()
            .ToListAsync();

        var items = list.Select(v => new
        {
            v.Id, Display = $"{v.Rate:P0}"
        });

        return new SelectList(items, "Id", "Display");
    }
}
