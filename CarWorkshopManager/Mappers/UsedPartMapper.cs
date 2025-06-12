using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.ViewModels.UsedPart;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class UsedPartMapper
{
    public partial UsedPart ToUsedPart(UsedPartFormViewModel vm);

    [MapProperty(nameof(UsedPart.Part.Name), nameof(UsedPartListItemViewModel.PartName))]
    public partial UsedPartListItemViewModel ToUsedPartListItem(UsedPart usedPart);
}
