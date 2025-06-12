using CarWorkshopManager.Models.Domain;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.ViewModels.ServiceTask;
using Riok.Mapperly.Abstractions;

namespace CarWorkshopManager.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ServiceTaskMapper
{
    public partial ServiceTask ToServiceTask(ServiceTaskFormViewModel vm);

    [MapProperty(nameof(ServiceTask.UsedParts), nameof(ServiceTaskListItemViewModel.Parts))]
    [MapProperty(nameof(ServiceTask.Mechanics), nameof(ServiceTaskListItemViewModel.Mechanics))]
    public partial ServiceTaskListItemViewModel ToServiceTaskListItemViewModel(ServiceTask task);

    private static List<string> MapMechanics(ICollection<ApplicationUser> mechanics)
        => mechanics.Select(m => $"{m.FirstName} {m.LastName}").ToList();
}
