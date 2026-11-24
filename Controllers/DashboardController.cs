using AutoVerifyMVC.Data.Repositories;
using AutoVerifyMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AutoVerifyMVC.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private static readonly HashSet<string> AllowedImagePaths = new(StringComparer.Ordinal)
    {
        "/assets/images/Default.png",
        "/assets/images/alto.png",
        "/assets/images/civic.png",
        "/assets/images/corolla.png",
        "/assets/images/kia.png",
        "/assets/images/LandCruiser.png",
        "/assets/images/Sonata%20Linen.png"
    };

    private readonly ICarRepository _carRepository;
    private readonly IInquiryRepository _inquiryRepository;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        ICarRepository carRepository,
        IInquiryRepository inquiryRepository,
        IDashboardRepository dashboardRepository,
        IAdminRepository adminRepository,
        UserManager<ApplicationUser> userManager)
    {
        _carRepository = carRepository;
        _inquiryRepository = inquiryRepository;
        _dashboardRepository = dashboardRepository;
        _adminRepository = adminRepository;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (User.IsInRole("Buyer")) return RedirectToAction(nameof(Buyer));
        if (User.IsInRole("Seller")) return RedirectToAction(nameof(Seller));
        if (User.IsInRole("Inspector")) return RedirectToAction(nameof(Inspector));
        if (User.IsInRole("Administrator")) return RedirectToAction(nameof(Admin));

        return Forbid();
    }

    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Buyer(CancellationToken cancellationToken)
    {
        var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var model = new BuyerDashboardViewModel
        {
            Cars = (await _carRepository.GetPublicListingsAsync(null, null, 0, 100, cancellationToken)).Cars,
            Inquiries = await _inquiryRepository.GetBuyerInquiriesAsync(buyerId, cancellationToken)
        };
        return View(model);
    }

    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Seller(CancellationToken cancellationToken)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var model = await BuildSellerDashboardAsync(sellerId, cancellationToken);
        return View(model);
    }

    [Authorize(Roles = "Inspector")]
    public async Task<IActionResult> Inspector(CancellationToken cancellationToken)
    {
        var inspectorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var model = await _inquiryRepository.GetInspectorDashboardAsync(inspectorId, cancellationToken);
        return View(model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Admin(string? userSearch, CancellationToken cancellationToken)
    {
        var model = await _adminRepository.GetDashboardAsync(userSearch, cancellationToken);
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AdminUsers(string? userSearch, CancellationToken cancellationToken)
    {
        ViewData["UserSearch"] = userSearch?.Trim() ?? string.Empty;
        return PartialView("_AdminUsersTable",
            await _adminRepository.GetUsersAsync(userSearch, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SetUserSuspension(
        string userId, bool suspend, string? userSearch, CancellationToken cancellationToken)
    {
        var target = await _userManager.FindByIdAsync(userId);
        if (target is null)
        {
            if (IsAjaxRequest()) return NotFound(new { success = false, message = "Account not found." });
            TempData["AdminError"] = "Account not found.";
            return RedirectToAction(nameof(Admin), new { userSearch });
        }

        if (target.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            if (IsAjaxRequest()) return BadRequest(new { success = false, message = "You cannot suspend your own administrator account." });
            TempData["AdminError"] = "You cannot suspend your own administrator account.";
            return RedirectToAction(nameof(Admin), new { userSearch });
        }

        if (suspend && await _userManager.IsInRoleAsync(target, "Administrator"))
        {
            var administrators = await _userManager.GetUsersInRoleAsync("Administrator");
            var otherActiveAdmins = 0;
            foreach (var administrator in administrators.Where(admin => admin.Id != target.Id))
            {
                if (!await _userManager.IsLockedOutAsync(administrator))
                    otherActiveAdmins++;
            }

            if (otherActiveAdmins == 0)
            {
                if (IsAjaxRequest()) return BadRequest(new { success = false, message = "The last active administrator cannot be suspended." });
                TempData["AdminError"] = "The last active administrator cannot be suspended.";
                return RedirectToAction(nameof(Admin), new { userSearch });
            }
        }

        IdentityResult result;
        if (suspend)
        {
            result = await _userManager.SetLockoutEnabledAsync(target, true);
            if (result.Succeeded)
                result = await _userManager.SetLockoutEndDateAsync(target, DateTimeOffset.MaxValue);
        }
        else
        {
            result = await _userManager.SetLockoutEndDateAsync(target, null);
        }

        if (result.Succeeded)
            result = await _userManager.UpdateSecurityStampAsync(target);

        var message = result.Succeeded
            ? suspend ? $"Account {target.Email} suspended." : $"Account {target.Email} restored."
            : string.Join(" ", result.Errors.Select(error => error.Description));
        if (IsAjaxRequest())
            return result.Succeeded ? Ok(new { success = true, suspended = suspend, message }) : BadRequest(new { success = false, message });
        TempData[result.Succeeded ? "AdminMessage" : "AdminError"] = message;
        return RedirectToAction(nameof(Admin), new { userSearch });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SetListingRemoval(
        int listingId, bool removed, CancellationToken cancellationToken)
    {
        var updated = await _adminRepository.SetListingRemovedAsync(listingId, removed, cancellationToken);
        if (IsAjaxRequest())
            return updated
                ? Ok(new { success = true, removed, status = removed ? "Removed" : "Unavailable", message = removed ? "Listing removed from public browsing." : "Listing restored as unavailable." })
                : NotFound(new { success = false, message = "The listing could not be updated." });
        TempData[updated ? "AdminMessage" : "AdminError"] = updated
            ? removed ? "Vehicle listing removed from public browsing." : "Listing restored as unavailable."
            : "The listing could not be updated.";
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SetInquiryCancellation(
        int inquiryId, bool cancelled, CancellationToken cancellationToken)
    {
        var updated = await _adminRepository.SetInquiryCancelledAsync(inquiryId, cancelled, cancellationToken);
        if (IsAjaxRequest())
            return updated
                ? Ok(new { success = true, cancelled, status = cancelled ? "Cancelled" : "Pending", inspectorName = "Unassigned", message = cancelled ? "Inspection request cancelled." : "Inspection request returned to the unassigned queue." })
                : Conflict(new { success = false, message = "This request could not be updated; completed requests cannot be cancelled." });
        TempData[updated ? "AdminMessage" : "AdminError"] = updated
            ? cancelled ? "Inspection request cancelled." : "Inspection request returned to the unassigned queue."
            : "This request could not be updated; completed requests cannot be cancelled.";
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateInspector(
        CreateInspectorAccountViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (IsAjaxRequest()) return BadRequest(new { success = false, message = "Enter a valid name, email, and temporary password (at least 8 characters)." });
            TempData["AdminError"] = "Enter a valid name, email, and temporary password (at least 8 characters).";
            return RedirectToAction(nameof(Admin));
        }

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            if (IsAjaxRequest()) return Conflict(new { success = false, message = "An account already uses that email address." });
            TempData["AdminError"] = "An account already uses that email address.";
            return RedirectToAction(nameof(Admin));
        }

        var inspector = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim(),
            EmailConfirmed = true
        };
        var created = await _userManager.CreateAsync(inspector, model.TemporaryPassword);
        if (!created.Succeeded)
        {
            var message = string.Join(" ", created.Errors.Select(error => error.Description));
            if (IsAjaxRequest()) return BadRequest(new { success = false, message });
            TempData["AdminError"] = message;
            return RedirectToAction(nameof(Admin));
        }

        var claimResult = await _userManager.AddClaimAsync(inspector, new Claim("FullName", inspector.FullName));
        var roleResult = claimResult.Succeeded
            ? await _userManager.AddToRoleAsync(inspector, "Inspector")
            : IdentityResult.Failed(claimResult.Errors.ToArray());
        if (!claimResult.Succeeded || !roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(inspector);
            var message = string.Join(" ", claimResult.Errors.Concat(roleResult.Errors).Select(error => error.Description));
            if (IsAjaxRequest()) return BadRequest(new { success = false, message });
            TempData["AdminError"] = message;
            return RedirectToAction(nameof(Admin));
        }

        var successMessage = $"Inspector account created for {inspector.Email}. Share the temporary password securely.";
        if (IsAjaxRequest()) return Ok(new { success = true, message = successMessage });
        TempData["AdminMessage"] = successMessage;
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Inspector")]
    public async Task<IActionResult> ClaimInspection(int inquiryId, CancellationToken cancellationToken)
    {
        var inspectorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var claimed = await _inquiryRepository.ClaimInspectionAsync(inquiryId, inspectorId, cancellationToken);
        if (IsAjaxRequest())
            return claimed ? Ok(new { success = true, message = "Inspection assigned to you." })
                : Conflict(new { success = false, message = "This request has already been claimed or is no longer pending." });
        TempData[claimed ? "InspectorMessage" : "InspectorError"] = claimed
            ? "Inspection assigned to you. Submit the report when it is complete."
            : "This request has already been claimed or is no longer pending.";
        return RedirectToAction(nameof(Inspector));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Inspector")]
    public async Task<IActionResult> SubmitInspectionReport(
        InspectionReportInputModel report, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (IsAjaxRequest()) return BadRequest(new { success = false, message = "Select a condition for each checklist item before submitting the report." });
            TempData["InspectorError"] = "Select a condition for each checklist item before submitting the report.";
            return RedirectToAction(nameof(Inspector));
        }

        var inspectorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var saved = await _inquiryRepository.SubmitInspectionReportAsync(report, inspectorId, cancellationToken);
        if (IsAjaxRequest())
            return saved ? Ok(new { success = true, message = "Inspection report submitted." })
                : Conflict(new { success = false, message = "This inspection is not assigned to your account or has already been completed." });
        TempData[saved ? "InspectorMessage" : "InspectorError"] = saved
            ? "Inspection report submitted."
            : "This inspection is not assigned to your account or has already been completed.";
        return RedirectToAction(nameof(Inspector));
    }

    [HttpGet]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> SearchCars(
        string? searchTerm, string? priceRange, CancellationToken cancellationToken)
    {
        var cars = await _carRepository.GetPublicListingsAsync(searchTerm, priceRange, 0, 100, cancellationToken);
        return PartialView("_BuyerCarCards", cars.Cars);
    }

    [HttpGet]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> BuyerInquiries(CancellationToken cancellationToken)
    {
        var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var inquiries = await _inquiryRepository.GetBuyerInquiriesAsync(buyerId, cancellationToken);
        return PartialView("_BuyerInquiries", inquiries);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> RequestInspection(
        InquiryRequestViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Please check the request details." });

        var car = await _carRepository.FindByIdAsync(model.CarListingId, cancellationToken);
        if (car is null || car.Status != "Available" || string.IsNullOrWhiteSpace(car.SellerId))
            return Conflict(new { success = false, message = "This vehicle is no longer available for inspection requests." });

        var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _inquiryRepository.CreateAsync(new CarInquiry
        {
            CarListingId = car.Id,
            BuyerId = buyerId,
            Message = model.Message?.Trim() ?? string.Empty,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return Ok(new { success = true, message = "Inspection request sent." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateListingStatus(
        UpdateListingStatusViewModel model, CancellationToken cancellationToken)
    {
        var allowedStatuses = new[] { "Available", "Reserved", "Unavailable", "Sold" };
        if (model.ListingId <= 0 || !allowedStatuses.Contains(model.Status))
            return BadRequest(new { success = false, message = "Choose a valid listing status." });

        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updated = await _carRepository.UpdateStatusAsync(
            model.ListingId, sellerId, model.Status, cancellationToken);
        if (!updated)
            return NotFound(new { success = false, message = "Listing not found or not available to this seller." });

        return Ok(new { success = true, status = model.Status, message = "Listing status updated." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateListing(
        [Bind(Prefix = "CreateListing")] CreateCarListingViewModel input,
        CancellationToken cancellationToken)
    {
        var imagePath = string.IsNullOrWhiteSpace(input.ImagePath)
            ? "/assets/images/Default.png"
            : input.ImagePath;
        if (!AllowedImagePaths.Contains(imagePath))
            ModelState.AddModelError(nameof(input.ImagePath), "Choose an image from the available vehicle photos.");

        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!ModelState.IsValid)
        {
            var dashboard = await BuildSellerDashboardAsync(sellerId, cancellationToken);
            dashboard.CreateListing = input;
            return View("Seller", dashboard);
        }

        await _carRepository.CreateAsync(new CarListing
        {
            SeedKey = $"seller-{Guid.NewGuid():N}",
            Make = input.Make.Trim(),
            Model = input.Model.Trim(),
            Trim = input.Trim.Trim(),
            ModelYear = input.ModelYear,
            Price = input.Price,
            MileageKm = input.MileageKm,
            Engine = input.Engine.Trim(),
            Transmission = input.Transmission.Trim(),
            FuelType = input.FuelType.Trim(),
            Description = input.Description.Trim(),
            ImagePath = imagePath,
            Status = "Available",
            SellerId = sellerId,
            IsSeedData = false,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        TempData["SellerMessage"] = "Your vehicle listing has been added.";
        return RedirectToAction(nameof(Seller));
    }

    private async Task<SellerDashboardViewModel> BuildSellerDashboardAsync(
        string sellerId, CancellationToken cancellationToken)
    {
        var stats = await _dashboardRepository.GetSellerStatsAsync(sellerId, cancellationToken);
        return new SellerDashboardViewModel
        {
            Cars = await _carRepository.GetSellerListingsAsync(sellerId, cancellationToken),
            Inquiries = await _inquiryRepository.GetSellerInquiriesAsync(sellerId, cancellationToken),
            ActiveListings = stats.ActiveListings,
            ReservedListings = stats.ReservedListings,
            UnavailableListings = stats.UnavailableListings,
            SoldListings = stats.SoldListings,
            TotalInquiries = stats.TotalInquiries,
            PendingInquiries = stats.PendingInquiries
        };
    }

    private bool IsAjaxRequest() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
