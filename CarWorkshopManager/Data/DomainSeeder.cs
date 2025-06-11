using CarWorkshopManager.Constants;
using CarWorkshopManager.Models.Domain;

namespace CarWorkshopManager.Data;

public static class DomainSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        foreach (var statusName in OrderStatuses.AllStatuses)
        {
            if (!db.OrderStatuses.Any(s => s.Name == statusName))
            {
                db.OrderStatuses.Add(new OrderStatus { Name = statusName });
            }
        }

        foreach (var (rate, code) in VatRates.AllVatRates)
        {
            if (!db.VatRates.Any(v => v.Rate == rate && v.Code == code))
            {
                db.VatRates.Add(new VatRate { Rate = rate, Code = code, ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow) });
            }
        }
        
        await db.SaveChangesAsync();
    }
}