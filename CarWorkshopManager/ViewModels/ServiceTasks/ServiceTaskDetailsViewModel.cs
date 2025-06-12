using System.Collections.Generic;
using CarWorkshopManager.Models.Domain;

namespace CarWorkshopManager.ViewModels.ServiceTasks
{
    public class ServiceTaskDetailsViewModel
    {
        public ServiceTask ServiceTask { get; set; }
        public List<TaskCommentViewModel> Comments { get; set; } = new List<TaskCommentViewModel>();
        public NewTaskCommentViewModel NewComment { get; set; }
    }
}
