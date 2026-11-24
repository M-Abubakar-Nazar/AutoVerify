namespace AutoVerifyMVC.Models;

public class CarListingViewModel
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Trim { get; set; } = string.Empty;
    public int ModelYear { get; set; }
    public decimal Price { get; set; }
    public int MileageKm { get; set; }
    public string Engine { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string? SellerId { get; set; }
    public bool IsSeedData { get; set; }
}

public class CarListingPageViewModel
{
    public IReadOnlyList<CarListingViewModel> Cars { get; set; } = [];
    public bool HasMore { get; set; }
}

public class CarStatusViewModel
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SellerId { get; set; }
}

public class InquiryViewModel
{
    public int Id { get; set; }
    public int CarListingId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string InspectorName { get; set; } = string.Empty;
    public string? InspectorId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? EngineCondition { get; set; }
    public string? TransmissionCondition { get; set; }
    public string? BodyCondition { get; set; }
    public string? BrakesCondition { get; set; }
    public string? InspectionNotes { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public class InspectorDashboardViewModel
{
    public int AvailableInspections { get; set; }
    public int ActiveInspections { get; set; }
    public int CompletedInspections { get; set; }
    public IReadOnlyList<InquiryViewModel> Queue { get; set; } = [];
    public IReadOnlyList<InquiryViewModel> AssignedInspections { get; set; } = [];
}

public class InspectorDashboardStats
{
    public int AvailableInspections { get; set; }
    public int ActiveInspections { get; set; }
    public int CompletedInspections { get; set; }
}

public class InspectionReportInputModel
{
    public int InquiryId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.RegularExpression("^(Good|Needs attention|Poor)$")]
    public string EngineCondition { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.RegularExpression("^(Good|Needs attention|Poor)$")]
    public string TransmissionCondition { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.RegularExpression("^(Good|Needs attention|Poor)$")]
    public string BodyCondition { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.RegularExpression("^(Good|Needs attention|Poor)$")]
    public string BrakesCondition { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string? InspectionNotes { get; set; }
}

public class AdminDashboardViewModel
{
    public string UserSearch { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int BuyerCount { get; set; }
    public int SellerCount { get; set; }
    public int InspectorCount { get; set; }
    public int AdministratorCount { get; set; }
    public int TotalListings { get; set; }
    public int AvailableListings { get; set; }
    public int PendingInquiries { get; set; }
    public int ActiveInspections { get; set; }
    public int CompletedInspections { get; set; }
    public int TotalInquiries { get; set; }
    public IReadOnlyList<AdminUserRowViewModel> Users { get; set; } = [];
    public IReadOnlyList<AdminInquiryRowViewModel> Inquiries { get; set; } = [];
    public IReadOnlyList<AdminListingRowViewModel> Listings { get; set; } = [];
}

public class AdminUserRowViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public bool IsSuspended { get; set; }
}

public class AdminInquiryRowViewModel
{
    public int Id { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string InspectorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class AdminListingRowViewModel
{
    public int Id { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRemoved => Status == "Removed";
}

public class CreateInspectorAccountViewModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(8)]
    public string TemporaryPassword { get; set; } = string.Empty;
}

public class BuyerDashboardViewModel
{
    public IReadOnlyList<CarListingViewModel> Cars { get; set; } = [];
    public IReadOnlyList<InquiryViewModel> Inquiries { get; set; } = [];
    public int PendingInquiries => Inquiries.Count(inquiry => inquiry.Status == "Pending");
    public int TotalInquiries => Inquiries.Count;
}

public class SellerDashboardViewModel
{
    public IReadOnlyList<CarListingViewModel> Cars { get; set; } = [];
    public IReadOnlyList<InquiryViewModel> Inquiries { get; set; } = [];
    public CreateCarListingViewModel CreateListing { get; set; } = new();
    public int ActiveListings { get; set; }
    public int ReservedListings { get; set; }
    public int UnavailableListings { get; set; }
    public int SoldListings { get; set; }
    public int TotalInquiries { get; set; }
    public int PendingInquiries { get; set; }
}

public class CreateCarListingViewModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(80)]
    public string Make { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Trim { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Range(1980, 2035)]
    public int ModelYear { get; set; }

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "1", "1000000000")]
    public decimal Price { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, 2000000)]
    public int MileageKm { get; set; }

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Engine { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(80)]
    public string Transmission { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string FuelType { get; set; } = "Petrol";

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    public string ImagePath { get; set; } = "/assets/images/Default.png";
}

public class InquiryRequestViewModel
{
    public int CarListingId { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string? Message { get; set; }
}

public class UpdateListingStatusViewModel
{
    public int ListingId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SellerDashboardStats
{
    public int ActiveListings { get; set; }
    public int ReservedListings { get; set; }
    public int UnavailableListings { get; set; }
    public int SoldListings { get; set; }
    public int TotalInquiries { get; set; }
    public int PendingInquiries { get; set; }
}

public sealed record CarPriceRangeBounds(
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    bool MinimumInclusive,
    bool MaximumInclusive);

public static class CarPriceRanges
{
    private const decimal TwentyLakh = 2_000_000m;
    private const decimal ThirtyLakh = 3_000_000m;
    private const decimal FortyLakh = 4_000_000m;
    private const decimal FiftyLakh = 5_000_000m;
    private const decimal SeventyLakh = 7_000_000m;
    private const decimal NinetyLakh = 9_000_000m;
    private const decimal OneTwentyLakh = 12_000_000m;

    public static CarPriceRangeBounds GetBounds(string? key) => key switch
    {
        "under-20" => new(null, TwentyLakh, false, false),
        "20-30" => new(TwentyLakh, ThirtyLakh, true, false),
        "30-40" => new(ThirtyLakh, FortyLakh, true, false),
        "40-50" => new(FortyLakh, FiftyLakh, true, false),
        "50-70" => new(FiftyLakh, SeventyLakh, true, false),
        "70-90" => new(SeventyLakh, NinetyLakh, true, false),
        "90-120" => new(NinetyLakh, OneTwentyLakh, true, true),
        "120-plus" => new(OneTwentyLakh, null, false, false),
        _ => new(null, null, false, false)
    };
}
