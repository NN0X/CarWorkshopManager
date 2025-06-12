namespace CarWorkshopManager.Constants
{
    public static class WorkRates
    {
        public const string Standard = "Standard";
        public const string Diagnostics = "Diagnostics";

        public static readonly (decimal HourRateNet, string Name)[] AllWorkRates =
        {
            (120m, Standard),
            (150m, Diagnostics),
        };
    }
}