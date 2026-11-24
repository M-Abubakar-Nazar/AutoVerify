using AutoVerifyMVC.Models;

namespace AutoVerifyMVC.Data.Repositories;

public interface IAdminRepository
{
    Task<AdminDashboardViewModel> GetDashboardAsync(string? userSearch = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminUserRowViewModel>> GetUsersAsync(string? userSearch = null, CancellationToken cancellationToken = default);
    Task<bool> SetListingRemovedAsync(int listingId, bool removed, CancellationToken cancellationToken = default);
    Task<bool> SetInquiryCancelledAsync(int inquiryId, bool cancelled, CancellationToken cancellationToken = default);
}
