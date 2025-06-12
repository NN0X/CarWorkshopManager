using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarWorkshopManager.ViewModels.ServiceTasks;
using CarWorkshopManager.ViewModels.UsedPart;

namespace CarWorkshopManager.ViewModels.ServiceOrder
{
    public class ServiceOrderDetailsViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DateTime ClosedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }

        public List<ServiceTaskListItemViewModel> Tasks { get; set; } = new();
        public ServiceTaskFormViewModel NewTask { get; set; } = new();
        public UsedPartFormViewModel NewUsedPart { get; set; } = new();

        public List<CommentViewModel> Comments { get; set; } = new();
        public string? NewCommentContent { get; set; }

        public IEnumerable<SelectListItem> WorkRates { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Parts { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Mechanics { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
    }
}
