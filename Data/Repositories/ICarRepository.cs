using AutoVerifyMVC.Models;

namespace AutoVerifyMVC.Data.Repositories;

public interface ICarRepository
{
    Task<CarListingPageViewModel> GetPublicListingsAsync(
        string? searchTerm, string? priceRange, int skip, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarStatusViewModel>> GetPublicStatusesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarListingViewModel>> GetSellerListingsAsync(
        string sellerId, CancellationToken cancellationToken = default);

    Task<CarListing?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        int id, string sellerId, string status, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(CarListing listing, CancellationToken cancellationToken = default);
}
