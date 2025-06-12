using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.ServiceTasks
{
    public class NewTaskCommentViewModel
    {
        [Required]
        public int ServiceTaskId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
    }
}
