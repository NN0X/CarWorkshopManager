namespace CarWorkshopManager.Constants;

public static class OrderStatuses
{
    public const string New = "New";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    
    public static readonly string[] AllStatuses = { New, InProgress, Completed };
}