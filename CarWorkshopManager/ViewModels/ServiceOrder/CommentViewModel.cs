namespace CarWorkshopManager.ViewModels.ServiceOrder
{
    public class CommentViewModel
    {
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
