using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarWorkshopManager.Services.Interfaces;

public interface IWorkRateService
{
    Task<SelectList> GetSelectWorkRatesAsync();
}