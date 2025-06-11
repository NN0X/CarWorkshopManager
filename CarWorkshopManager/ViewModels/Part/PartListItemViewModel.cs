namespace CarWorkshopManager.ViewModels.Part;

public class PartListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPriceNet { get; set; }
    public string VatRateCode { get; set; } = string.Empty;
}