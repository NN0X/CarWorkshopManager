using CarWorkshopManager.Constants;
using CarWorkshopManager.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Data;

public static class DomainSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var existingStatusNames = await db.OrderStatuses
                                          .Select(s => s.Name)
                                          .ToListAsync();
        var statusesToAdd = OrderStatuses.AllStatuses
                                         .Where(name => !existingStatusNames.Contains(name))
                                         .Select(name => new OrderStatus { Name = name })
                                         .ToList();
        if (statusesToAdd.Any())
        {
            db.OrderStatuses.AddRange(statusesToAdd);
        }

        var existingVatCodes = await db.VatRates
                                       .Select(v => v.Code)
                                       .ToListAsync();
        var vatToAdd = VatRates.AllVatRates
                               .Where(tuple => !existingVatCodes.Contains(tuple.Code))
                               .Select(tuple => new VatRate
                               {
                                   Rate = tuple.Rate,
                                   Code = tuple.Code,
                                   ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow)
                               })
                               .ToList();
        if (vatToAdd.Any())
        {
            db.VatRates.AddRange(vatToAdd);
        }

        await db.SaveChangesAsync();

        var defaultVat23 = await db.VatRates
                                  .FirstOrDefaultAsync(v => v.Code == VatRates.Vat23);
        if (defaultVat23 == null)
        {
            var tuple23 = VatRates.AllVatRates
                                  .FirstOrDefault(t => t.Code == VatRates.Vat23);
            decimal rate23 = tuple23 != default ? tuple23.Rate : 0.23M;
            defaultVat23 = new VatRate
            {
                Code = VatRates.Vat23,
                Rate = rate23,
                ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            db.VatRates.Add(defaultVat23);
            await db.SaveChangesAsync();
        }

        var existingWorkNames = await db.WorkRates
                                        .Select(w => w.Name)
                                        .ToListAsync();
        var workRatesToAdd = WorkRates.AllWorkRates
                                      .Where(tuple => !existingWorkNames.Contains(tuple.Name))
                                      .Select(tuple => new WorkRate
                                      {
                                          Name = tuple.Name,
                                          HourRateNet = tuple.HourRateNet,
                                          VatRate = defaultVat23,
                                          ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow)
                                      })
                                      .ToList();
        if (workRatesToAdd.Any())
        {
            db.WorkRates.AddRange(workRatesToAdd);
            await db.SaveChangesAsync();
        }
    }
}
