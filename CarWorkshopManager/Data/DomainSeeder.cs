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
        
        await db.SaveChangesAsync();
    }
}