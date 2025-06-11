using CarWorkshopManager.Constants;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.ServiceOrder;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class ServiceOrderService : IServiceOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly ServiceOrderMapper _mapper;

    public ServiceOrderService(ApplicationDbContext db, ServiceOrderMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<int> CreateOrderAsync(CreateServiceOrderViewModel model, string userId)
    {
        var vehicle = await _db.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == model.VehicleId);

        if (vehicle is null)
            throw new Exception("Pojazd nie istnieje");

        var statusId = await _db.OrderStatuses
            .Where(s => s.Name == OrderStatuses.New)
            .Select(s => s.Id)
            .FirstAsync();

        var serviceOrder = _mapper.ToServiceOrder(model);
        serviceOrder.CreatedById = userId;
        serviceOrder.StatusId = statusId;
        serviceOrder.CustomerNameSnapshot = $"{vehicle.Customer.FirstName} {vehicle.Customer.LastName}";
        serviceOrder.RegistrationNumberSnapshot = vehicle.RegistrationNumber;
        
        _db.ServiceOrders.Add(serviceOrder);
        await _db.SaveChangesAsync();

        return serviceOrder.Id;
    }
}