using CarWorkshopManager.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManager.Models.Domain;

namespace CarWorkshopManager.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<OrderComment> OrderComments { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }
    public DbSet<ServiceTask> ServiceTasks { get; set; }
    public DbSet<UsedPart> UsedParts { get; set; }
    public DbSet<VatRate> VatRates { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleBrand> VehicleBrands { get; set; }
    public DbSet<WorkRate> WorkRates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.PhoneNumber).IsUnique();
        });

        builder.Entity<OrderStatus>(entity =>
        {
            entity.HasIndex(os => os.Name).IsUnique();
        });

        builder.Entity<Part>(entity =>
        {
            entity.HasIndex(part => part.Name).IsUnique();
        });
        
        builder.Entity<ServiceOrder>(entity =>
        {
            entity.HasIndex(so => so.OrderNumber).IsUnique();
        });

        builder.Entity<UsedPart>(entity =>
        {
            entity.HasIndex(up => new { up.ServiceTaskId, up.PartId }).IsUnique();
        });

        builder.Entity<VatRate>(entity =>
        {
            entity.HasIndex(vr => vr.Code).IsUnique();
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(v => v.Vin).IsUnique();
            entity.HasIndex(v => v.RegistrationNumber).IsUnique();
        });

        builder.Entity<VehicleBrand>(entity =>
        {
            entity.HasIndex(vb => vb.Name).IsUnique();
        });

        builder.Entity<WorkRate>(entity =>
        {
            entity.HasIndex(wr => wr.Name).IsUnique();
        });
    }
}