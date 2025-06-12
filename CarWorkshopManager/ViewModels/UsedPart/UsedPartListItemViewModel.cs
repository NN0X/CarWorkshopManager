namespace CarWorkshopManager.ViewModels.UsedPart;

public class UsedPartListItemViewModel
{
    public int Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceNetSnapshot { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
}