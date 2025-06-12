using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Part;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarWorkshopManager.Services.Interfaces;

public interface IPartService
{
    Task<List<PartListItemViewModel>> GetAllPartsAsync();
    Task<PartFormViewModel?> GetPartByIdAsync(int id);
    Task CreatePartAsync(PartFormViewModel partFormViewModel);
    Task SoftDeletePartAsync(int id);
    Task UpdatePartAsync(PartFormViewModel partFormViewModel);
    Task<SelectList> GetActivePartsSelectAsync(); 
}