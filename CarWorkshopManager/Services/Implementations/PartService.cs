using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Services.Interfaces;
using CarWorkshopManager.ViewModels.Part;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Services.Implementations;

public class PartService : IPartService
{
    private readonly ApplicationDbContext _db;
    private readonly PartMapper _mapper;

    public PartService(ApplicationDbContext db, PartMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }
    
    public async Task<List<PartListItemViewModel>> GetAllPartsAsync()
    {
        var parts = await _db.Parts
            .Where(p => p.IsActive)
            .Include(p => p.VatRate)
            .ToListAsync();

        return parts.Select(_mapper.ToPartListItemViewModel).ToList();
    }
    
    public async Task<PartFormViewModel?> GetPartByIdAsync(int id)
    {
        var part = await _db.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        return part is null ? null : _mapper.ToPartFormViewModel(part);
    }
    
    public async Task CreatePartAsync(PartFormViewModel vm)
    {
        var part = _mapper.ToPart(vm);
        await _db.Parts.AddAsync(part);
        await _db.SaveChangesAsync();
    }
    
    public async Task UpdatePartAsync(PartFormViewModel vm)
    {
        var part = await _db.Parts
            .FirstOrDefaultAsync(p => p.Id == vm.Id && p.IsActive);
        if (part is null) 
            return;

        _mapper.MapToExisting(vm, part);   
        await _db.SaveChangesAsync();
    }
    
    public async Task SoftDeletePartAsync(int id)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part is null) 
            return;

        part.IsActive = false;
        await _db.SaveChangesAsync();
    }
}
