using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Customer;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _db;
    private readonly CustomerMapper _mapper;
    private readonly IVehicleService _vehicleService;

    public CustomerService(ApplicationDbContext db, CustomerMapper mapper, IVehicleService vehicleService)
    {
        _db = db;
        _mapper = mapper;
        _vehicleService = vehicleService;
    }

    public async Task AddCustomerAsync(CreateCustomerViewModel model)
    {
        var customer = _mapper.ToCustomer(model);
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
    }

    public async Task<List<CustomerListItemViewModel>> GetAllCustomersAsync()
    {
        var customers = await _db.Customers.OrderBy(c => c.Id).ToListAsync();
        return customers.Select(_mapper.ToCreateCustomerListItemViewModel).ToList();
    }

    public async Task<CustomerListItemViewModel?> GetCustomerAsync(int id)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
            return null;

        return _mapper.ToCreateCustomerListItemViewModel(customer);
    }
    public async Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) 
            return null;

        var vehicles = await _vehicleService.GetCustomerVehicles(id);

        return new CustomerDetailsViewModel
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Vehicles = vehicles
        };
    }
}
