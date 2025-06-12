namespace CarWorkshopManager.ViewModels.ServiceOrder;
public class MonthlyRepairSummaryItemViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalCostNet { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalCostGross => TotalCostNet + TotalVat;
}
