using AutoVerifyMVC.Models;

namespace AutoVerifyMVC.Data.Repositories;

public interface IDashboardRepository
{
    Task<SellerDashboardStats> GetSellerStatsAsync(
        string sellerId, CancellationToken cancellationToken = default);
}
