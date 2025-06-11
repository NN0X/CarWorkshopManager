using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Part;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PartMapper
{
    public partial Part ToPart(CreatePartViewModel createPartViewModel);
    
    [MapperIgnoreSource(nameof(Part.VatRate))] 
    [MapperIgnoreSource(nameof(Part.UsedParts))] 
    public partial PartListItemViewModel ToPartListItemViewModel(Part part);
}