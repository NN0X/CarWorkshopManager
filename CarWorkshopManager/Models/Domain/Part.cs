﻿using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Models.Domain;

public class Part
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Precision(10, 2)]
    [Range(0.01, 10000000)]
    public decimal UnitPriceNet { get; set; }

    [Required]
    public int VatRateId { get; set; }

    public bool IsActive { get; set; } = true;

    public VatRate VatRate { get; set; } = null!;
    public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
}
