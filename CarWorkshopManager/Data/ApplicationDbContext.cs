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


        builder.Entity<Vehicle>(entity =>
        {
            entity.HasOne(v => v.Customer)
                  .WithMany(c => c.Vehicles)
                  .HasForeignKey(v => v.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.HasOne(v => v.VehicleBrand)
                  .WithMany(vb => vb.Vehicles)
                  .HasForeignKey(v => v.VehicleBrandId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Part>(entity =>
        {
            entity.HasOne(p => p.VatRate)
                .WithMany()
                .HasForeignKey(p => p.VatRateId)
                .OnDelete(DeleteBehavior.Restrict);

        });
        
        builder.Entity<ServiceOrder>(entity =>
        {
            entity.HasOne(so => so.Vehicle)
                  .WithMany()
                  .HasForeignKey(so => so.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ServiceOrder>(entity =>
        {
            entity.HasOne(so => so.Status)
                  .WithMany()
                  .HasForeignKey(so => so.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ServiceOrder>(entity =>
        {
            entity.HasOne(so => so.CreatedBy)
                  .WithMany()
                  .HasForeignKey(so => so.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<ServiceTask>(entity =>
        {
            entity.HasOne(st => st.WorkRate)
                  .WithMany()
                  .HasForeignKey(st => st.WorkRateId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ServiceTask>(entity =>
        {
            entity.HasOne(st => st.ServiceOrder)
                .WithMany(o => o.Tasks)
                .HasForeignKey(st => st.ServiceOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<UsedPart>(entity =>
        {
            entity.HasOne(up => up.Part)
                .WithMany(p => p.UsedParts)
                .HasForeignKey(up => up.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UsedPart>(entity =>
        {
            entity.HasOne(up => up.ServiceTask)
                .WithMany(t => t.UsedParts)
                .HasForeignKey(up => up.ServiceTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        

        builder.Entity<WorkRate>(entity =>
        {
            entity.HasOne(wr => wr.VatRate)
                  .WithMany()
                  .HasForeignKey(wr => wr.VatRateId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderComment>(entity =>
        {
            entity.HasOne(oc => oc.Author)
                  .WithMany()
                  .HasForeignKey(oc => oc.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
