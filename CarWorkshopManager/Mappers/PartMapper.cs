using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.Part;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PartMapper
{
    public partial Part ToPart(PartFormViewModel partFormViewModel);
    public partial PartFormViewModel ToPartFormViewModel(Part part); 

    [MapProperty(nameof(Part.VatRate.Rate), nameof(PartListItemViewModel.VatRateValue))]
    public partial PartListItemViewModel ToPartListItemViewModel(Part part);

    public partial void MapToExisting(PartFormViewModel partFormViewModel, [MappingTarget] Part part);
}
