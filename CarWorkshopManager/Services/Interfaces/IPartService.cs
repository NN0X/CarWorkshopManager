using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Part;

namespace CarWorkshopManager.Services.Interfaces;

public interface IPartService
{
    Task<List<PartListItemViewModel>> GetAllPartsAsync();
    Task CreatePartAsync(CreatePartViewModel createPartViewModel);
    Task SoftDeletePartAsync(int id);
}