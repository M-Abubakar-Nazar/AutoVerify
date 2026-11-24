using System.ComponentModel.DataAnnotations;

namespace AutoVerifyMVC.Models;

public class CarInquiry
{
    public int Id { get; set; }

    public int CarListingId { get; set; }
    public CarListing CarListing { get; set; } = null!;

    [Required]
    public string BuyerId { get; set; } = string.Empty;
    public ApplicationUser Buyer { get; set; } = null!;

    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? InspectorId { get; set; }
    public ApplicationUser? Inspector { get; set; }

    [MaxLength(30)]
    public string? EngineCondition { get; set; }

    [MaxLength(30)]
    public string? TransmissionCondition { get; set; }

    [MaxLength(30)]
    public string? BodyCondition { get; set; }

    [MaxLength(30)]
    public string? BrakesCondition { get; set; }

    [MaxLength(2000)]
    public string? InspectionNotes { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}
