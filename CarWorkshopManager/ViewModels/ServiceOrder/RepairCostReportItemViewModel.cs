using System;

namespace CarWorkshopManager.ViewModels.ServiceOrder
{
    public class RepairCostReportItemViewModel
    {
        public int ServiceOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal LaborNet { get; set; }
        public decimal PartsNet { get; set; }
        public decimal TotalNet => LaborNet + PartsNet;
        public decimal TotalVat { get; set; }
    }
}
