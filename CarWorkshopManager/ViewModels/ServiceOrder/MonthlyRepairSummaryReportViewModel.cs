namespace CarWorkshopManager.ViewModels.ServiceOrder;
public class MonthlyRepairSummaryReportViewModel
{
    public DateTime Month { get; set; }
    public List<MonthlyRepairSummaryItemViewModel> Items { get; set; } = new();
}
