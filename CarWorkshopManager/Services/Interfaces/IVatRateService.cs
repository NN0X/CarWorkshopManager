using CarWorkshopManager.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarWorkshopManager.Services.Interfaces;

public interface IVatRateService
{
    Task<SelectList>GetSelectVatRatesListAsync();
}