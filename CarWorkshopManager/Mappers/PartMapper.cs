using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Part;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PartMapper
{
    public partial Part ToPart(PartFormViewModel partFormViewModel);
    
    [MapProperty(nameof(Part.VatRate.Code), nameof(PartListItemViewModel.VatRateCode))]
    public partial PartListItemViewModel ToPartListItemViewModel(Part part);
}