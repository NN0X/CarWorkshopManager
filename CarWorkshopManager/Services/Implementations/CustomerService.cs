using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _db;
        private readonly CustomerMapper     _mapper;
        private readonly IVehicleService    _vehicleService;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ApplicationDbContext db,
            CustomerMapper mapper,
            IVehicleService vehicleService,
            ILogger<CustomerService> logger)
        {
            _db = db;
            _mapper = mapper;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        public async Task AddCustomerAsync(CreateCustomerViewModel model)
        {
            _logger.LogInformation("Adding customer {FirstName} {LastName}", model.FirstName, model.LastName);
            try
            {
                var entity = _mapper.ToCustomer(model);
                _db.Customers.Add(entity);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Customer added with Id={CustomerId}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer {FirstName} {LastName}", model.FirstName, model.LastName);
                throw;
            }
        }

        public async Task<List<CustomerListItemViewModel>> GetAllCustomersAsync()
        {
            _logger.LogInformation("Retrieving all customers");
            var list = await _db.Customers.OrderBy(c => c.Id).ToListAsync();
            _logger.LogInformation("Retrieved {Count} customers", list.Count);
            return list.Select(_mapper.ToCreateCustomerListItemViewModel).ToList();
        }

        public async Task<CustomerListItemViewModel?> GetCustomerAsync(int id)
        {
            _logger.LogInformation("Getting customer by Id={CustomerId}", id);
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
            {
                _logger.LogWarning("Customer not found: Id={CustomerId}", id);
                return null;
            }
            return _mapper.ToCreateCustomerListItemViewModel(customer);
        }

        public async Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id)
        {
            _logger.LogInformation("Getting customer details for Id={CustomerId}", id);
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
            {
                _logger.LogWarning("Customer not found: Id={CustomerId}", id);
                return null;
            }

            var vehicles = await _vehicleService.GetCustomerVehiclesAsync(id);
            _logger.LogInformation("Found {VehicleCount} vehicles for customer {CustomerId}", vehicles.Count, id);

            return new CustomerDetailsViewModel
            {
                Id          = customer.Id,
                FirstName   = customer.FirstName,
                LastName    = customer.LastName,
                Email       = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Vehicles    = vehicles
            };
        }
    }
}
