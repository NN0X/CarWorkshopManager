using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarWorkshopManager.ViewModels.ServiceOrder
{
    public class RepairCostReportViewModel
    {
        public int? VehicleId { get; set; }
        public DateTime? Month { get; set; }
        public IEnumerable<SelectListItem> Vehicles { get; set; } = new List<SelectListItem>();
        public IEnumerable<RepairCostReportItemViewModel> Items { get; set; } = new List<RepairCostReportItemViewModel>();
    }
}
