namespace CarWorkshopManager.Constants;

public static class VatRates
{
    public const string Vat8 = "VAT8";
    public const string Vat23 = "VAT23";

    public static readonly (decimal Rate, string Code)[] AllVatRates =
    {
        (0.08M, Vat8),
        (0.23M, Vat23),
    };
}