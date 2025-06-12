using System;

namespace CarWorkshopManager.ViewModels.ServiceTasks
{
    public class TaskCommentViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
