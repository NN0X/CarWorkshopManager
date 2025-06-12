using CarWorkshopManager.Constants;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceTask;
using CarWorkshopManager.ViewModels.UsedPart;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class ServiceTaskService : IServiceTaskService
{
    private readonly ApplicationDbContext _db;
    private readonly ServiceTaskMapper _serviceTaskMapper;
    private readonly UsedPartMapper _usedPartMapper;
    UserManager<ApplicationUser> _userManager;

    public ServiceTaskService(ApplicationDbContext db, ServiceTaskMapper serviceTaskMapper,
        UsedPartMapper usedPartMapper, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _serviceTaskMapper = serviceTaskMapper;
        _usedPartMapper = usedPartMapper;
        _userManager = userManager;
    }

    public async Task<int> AddServiceTaskAsync(ServiceTaskFormViewModel model)
    {
        if (!await _db.ServiceOrders.AnyAsync(o => o.Id == model.ServiceOrderId))
            throw new KeyNotFoundException("Zlecenie nie istnieje");
        
        var wr = await _db.WorkRates
                     .Include(w => w.VatRate)
                     .FirstOrDefaultAsync(w => w.Id == model.WorkRateId)
                 ?? throw new KeyNotFoundException("Stawka robocizny nie istnieje");
        
        var task = _serviceTaskMapper.ToServiceTask(model);
        task.HourRateNetSnapshot = wr.HourRateNet;
        task.VatRateSnapshot = wr.VatRate.Rate;
        
        if (model.MechanicsIds.Any())
        {
            var mechanics = await _db.Users
                .Where(u => model.MechanicsIds.Contains(u.Id))
                .ToListAsync();
            
            foreach (var m in mechanics)
                task.Mechanics.Add(m);
        }
        
        var mechCount = task.Mechanics.Count > 0 ? task.Mechanics.Count : 1;
        task.TotalNet = task.WorkHours * task.HourRateNetSnapshot * mechCount;
        task.TotalVat = task.TotalNet * task.VatRateSnapshot;
        
        _db.ServiceTasks.Add(task);
        await _db.SaveChangesAsync();
        
        await RecalculateTotalsAsync(task.ServiceOrderId);

        return task.Id;
    }

    public async Task<int> AddUsedPartAsync(UsedPartFormViewModel model, string currentUserId)
    {
        var task = await _db.ServiceTasks
                       .Include(t => t.Mechanics)
                       .FirstOrDefaultAsync(t => t.Id == model.ServiceTaskId)
                   ?? throw new KeyNotFoundException("Czynność serwisowa nie istnieje.");
        
        var user = await _userManager.FindByIdAsync(currentUserId)
                   ?? throw new KeyNotFoundException("Niepoprawny użytkownik.");

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        var isMechanicOfTask = task.Mechanics.Any(m => m.Id == currentUserId);

        if (!isAdmin && !isMechanicOfTask)
            throw new UnauthorizedAccessException("Brak uprawnień do dodania części.");

        var part = await _db.Parts
                       .Include(p => p.VatRate)
                       .FirstOrDefaultAsync(p => p.Id == model.PartId && p.IsActive)
                   ?? throw new KeyNotFoundException("Część nie istnieje lub jest nieaktywna");
        
        var used = _usedPartMapper.ToUsedPart(model);
        used.UnitPriceNetSnapshot = part.UnitPriceNet;
        used.VatRateSnapshot = part.VatRate.Rate;
        used.TotalNet = used.Quantity * used.UnitPriceNetSnapshot;
        used.TotalVat = used.TotalNet * used.VatRateSnapshot;
        
        _db.UsedParts.Add(used);
        await _db.SaveChangesAsync();
        
        await RecalculateTotalsAsync(task.ServiceOrderId);
        
        return used.Id;
    }

    public async Task<int> GetTaskServiceOrderServiceIdAsync(int serviceTaskId)
    {
        var task = await _db.ServiceTasks.FindAsync(serviceTaskId)
                   ?? throw new KeyNotFoundException("Czynnośc serwisowa nie istnieje.");
        return task.ServiceOrderId;
    }


    public async Task RecalculateTotalsAsync(int serviceOrderId)
    {
        var order = await _db.ServiceOrders
                        .Include(o => o.Tasks)
                        .ThenInclude(t => t.UsedParts)
                        .FirstOrDefaultAsync(o => o.Id == serviceOrderId)
                    ?? throw new KeyNotFoundException("Zlecenie nie istnieje");
        
        var laborNet = order.Tasks.Sum(t => t.TotalNet);
        var laborVat = order.Tasks.Sum(t => t.TotalVat);
        
        var partsNet = order.Tasks
            .SelectMany(t => t.UsedParts)
            .Sum(up => up.TotalNet);
        var partsVat = order.Tasks
            .SelectMany(t => t.UsedParts)
            .Sum(up => up.TotalVat);

        order.TotalNet = laborNet + partsNet;
        order.TotalVat = laborVat + partsVat;

        await _db.SaveChangesAsync();
    }
}