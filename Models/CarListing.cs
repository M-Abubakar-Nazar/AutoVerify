using System.ComponentModel.DataAnnotations;

namespace AutoVerifyMVC.Models;

public class CarListing
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string SeedKey { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Make { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Trim { get; set; } = string.Empty;

    public int ModelYear { get; set; }

    public decimal Price { get; set; }

    public int MileageKm { get; set; }

    [Required, MaxLength(100)]
    public string Engine { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Transmission { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string FuelType { get; set; } = "Petrol";

    [Required, MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string ImagePath { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Available";

    public bool IsSeedData { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? SellerId { get; set; }
    public ApplicationUser? Seller { get; set; }
}
