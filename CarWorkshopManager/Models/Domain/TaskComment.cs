using System;
using System.ComponentModel.DataAnnotations;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Models.Domain;

namespace CarWorkshopManager.Models.Domain
{
    public class TaskComment
    {
        public int Id { get; set; }

        public int ServiceTaskId { get; set; }
        public ServiceTask ServiceTask { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
