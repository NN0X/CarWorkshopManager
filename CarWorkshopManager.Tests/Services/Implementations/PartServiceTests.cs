using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.ViewModels.Part;
using CarWorkshopManager.Models.Domain;           
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

namespace CarWorkshopManager.Tests.Services.Implementations
{
    public class PartServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly PartService _service;

        public PartServiceTests()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(opts);
            _service = new PartService(_db, new PartMapper());
        }

        public void Dispose() => _db.Dispose();
        
        
        [Fact]
        public async Task GetAllPartsAsync_ReturnsOnlyActiveParts()
        {
            var active = new Part { Id = 1, Name = "A", UnitPriceNet = 10, IsActive = true,  VatRate = new VatRate { Rate = 0.23m } };
            var inactive = new Part { Id = 2, Name = "B", UnitPriceNet = 20, IsActive = false, VatRate = new VatRate { Rate = 0.08m } };
            _db.Parts.AddRange(active, inactive);
            await _db.SaveChangesAsync();

            var result = await _service.GetAllPartsAsync();

            Assert.Single(result);
            Assert.Equal("A", result[0].Name);
        }
        
        [Fact]
        public async Task GetPartByIdAsync_InactiveOrMissing_ReturnsNull()
        {
            _db.Parts.Add(new Part { Id = 7, Name = "X", IsActive = false });
            await _db.SaveChangesAsync();

            Assert.Null(await _service.GetPartByIdAsync(7));     
            Assert.Null(await _service.GetPartByIdAsync(9999));  
        }

        [Fact]
        public async Task GetPartByIdAsync_Active_ReturnsMappedVm()
        {
            var part = new Part { Id = 10, Name = "Bolt", UnitPriceNet = 3, IsActive = true, VatRateId = 1, VatRate = new VatRate { Rate = 0.23m } };
            _db.Parts.Add(part);
            await _db.SaveChangesAsync();

            var vm = await _service.GetPartByIdAsync(10);

            Assert.NotNull(vm);
            Assert.Equal("Bolt", vm.Name);
            Assert.Equal(3, vm.UnitPriceNet);
            Assert.Equal(1, vm.VatRateId);
        }
        
        [Fact]
        public async Task CreatePartAsync_AddsEntityToDb()
        {
            var vm = new PartFormViewModel { Name = "Nut", UnitPriceNet = 4, VatRateId = 1 };

            await _service.CreatePartAsync(vm);

            var saved = await _db.Parts.SingleAsync();
            Assert.Equal("Nut", saved.Name);
            Assert.True(saved.IsActive);
        }

        [Fact]
        public async Task UpdatePartAsync_WhenActive_UpdatesProperties()
        {
            var part = new Part { Id = 200, Name = "Old", UnitPriceNet = 5, IsActive = true, VatRateId = 1 };
            _db.Parts.Add(part);
            await _db.SaveChangesAsync();

            var vm = new PartFormViewModel { Id = 200, Name = "New", UnitPriceNet = 6, VatRateId = 1 };

            await _service.UpdatePartAsync(vm);

            Assert.Equal("New", part.Name);
            Assert.Equal(6, part.UnitPriceNet);
        }

        [Fact]
        public async Task UpdatePartAsync_WhenInactive_DoesNothing()
        {
            var part = new Part { Id = 201, Name = "X", UnitPriceNet = 1, IsActive = false, VatRateId = 1 };
            _db.Parts.Add(part);
            await _db.SaveChangesAsync();

            var vm = new PartFormViewModel { Id = 201, Name = "Change", UnitPriceNet = 2, VatRateId = 1 };

            await _service.UpdatePartAsync(vm);

            Assert.Equal("X", part.Name);
            Assert.Equal(1, part.UnitPriceNet);
        }
        
        [Fact]
        public async Task SoftDeletePartAsync_SetsIsActiveFalse()
        {
            var part = new Part { Id = 300, Name = "Del", UnitPriceNet = 9, IsActive = true };
            _db.Parts.Add(part);
            await _db.SaveChangesAsync();

            await _service.SoftDeletePartAsync(300);

            Assert.False(part.IsActive);
        }
        
        [Fact]
        public async Task GetActivePartsSelectAsync_ReturnsOrderedSelectList()
        {
            _db.Parts.AddRange(
                new Part { Id = 1, Name = "Zeta",  IsActive = true },
                new Part { Id = 2, Name = "Alpha", IsActive = true },
                new Part { Id = 3, Name = "Beta",  IsActive = false }
            );
            await _db.SaveChangesAsync();

            var select = await _service.GetActivePartsSelectAsync();
            var items  = select.Cast<SelectListItem>().ToList();

            Assert.Equal(2, items.Count);
            Assert.Equal("Alpha", items[0].Text);   
            Assert.Equal("Zeta", items[1].Text);
            Assert.Equal("1", items[1].Value);  
        }
    }
}
