namespace CarWorkshopManager.ViewModels.ServiceOrder;

public class ServiceOrderListItemViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime ClosedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
}