using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Part;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWorkshopManager.Services.Implementations
{
    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _db;
        private readonly PartMapper _mapper;
        private readonly ILogger<PartService> _logger;

        public PartService(
            ApplicationDbContext db,
            PartMapper mapper,
            ILogger<PartService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PartListItemViewModel>> GetAllPartsAsync()
        {
            _logger.LogInformation("GetAllPartsAsync called");
            var parts = await _db.Parts
                .Where(p => p.IsActive)
                .Include(p => p.VatRate)
                .ToListAsync();
            return parts.Select(_mapper.ToPartListItemViewModel).ToList();
        }

        public async Task<PartFormViewModel?> GetPartByIdAsync(int id)
        {
            _logger.LogInformation("GetPartByIdAsync called for Id={Id}", id);
            var part = await _db.Parts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (part is null)
            {
                _logger.LogWarning("GetPartByIdAsync: not found Id={Id}", id);
                return null;
            }
            return _mapper.ToPartFormViewModel(part);
        }

        public async Task CreatePartAsync(PartFormViewModel vm)
        {
            _logger.LogInformation("CreatePartAsync called: {Name}", vm.Name);
            var part = _mapper.ToPart(vm);
            await _db.Parts.AddAsync(part);
            await _db.SaveChangesAsync();
            _logger.LogInformation("CreatePartAsync: part created Id={Id}", part.Id);
        }

        public async Task UpdatePartAsync(PartFormViewModel vm)
        {
            _logger.LogInformation("UpdatePartAsync called: Id={Id}", vm.Id);
            var part = await _db.Parts
                .FirstOrDefaultAsync(p => p.Id == vm.Id && p.IsActive);
            if (part is null)
            {
                _logger.LogWarning("UpdatePartAsync: not found Id={Id}", vm.Id);
                return;
            }
            _mapper.MapToExisting(vm, part);
            await _db.SaveChangesAsync();
            _logger.LogInformation("UpdatePartAsync: updated Id={Id}", vm.Id);
        }

        public async Task SoftDeletePartAsync(int id)
        {
            _logger.LogInformation("SoftDeletePartAsync called for Id={Id}", id);
            var part = await _db.Parts.FindAsync(id);
            if (part is null)
            {
                _logger.LogWarning("SoftDeletePartAsync: not found Id={Id}", id);
                return;
            }
            part.IsActive = false;
            await _db.SaveChangesAsync();
            _logger.LogInformation("SoftDeletePartAsync: deactivated Id={Id}", id);
        }

        public async Task<SelectList> GetActivePartsSelectAsync()
        {
            _logger.LogInformation("GetActivePartsSelectAsync called");  
            var list = await _db.Parts
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();
            return new SelectList(list, "Id", "Name");
        }
    }
}
