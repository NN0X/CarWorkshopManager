using CarWorkshopManager.ViewModels.UsedPart;

namespace CarWorkshopManager.ViewModels.ServiceTasks;

public class ServiceTaskListItemViewModel
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal WorkHours  { get; set; }
    public decimal HourRateNetSnapshot { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }

    public List<string> Mechanics { get; set; } = new();
    public List<UsedPartListItemViewModel> Parts { get; set; } = new();
}
