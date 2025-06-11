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
        _db = db;
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

    public async Task CreatePartAsync(PartFormViewModel partFormViewModel)
    {
        var part = _mapper.ToPart(partFormViewModel);
        await _db.Parts.AddAsync(part);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeletePartAsync(int id)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part is not null)
        {
            part.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}