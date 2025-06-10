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

    public CustomerService(ApplicationDbContext db, CustomerMapper mapper)
    {
        _db = db;
        _mapper = mapper;
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
}