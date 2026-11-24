using AutoVerifyMVC.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace AutoVerifyMVC.Data.Repositories;

public class DashboardRepository(ApplicationDbContext dbContext) : IDashboardRepository
{
    public async Task<SellerDashboardStats> GetSellerStatsAsync(
        string sellerId, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition("""
            SELECT
                COALESCE(SUM(CASE WHEN c.Status = N'Available' THEN 1 ELSE 0 END), 0) AS ActiveListings,
                COALESCE(SUM(CASE WHEN c.Status = N'Reserved' THEN 1 ELSE 0 END), 0) AS ReservedListings,
                COALESCE(SUM(CASE WHEN c.Status = N'Unavailable' THEN 1 ELSE 0 END), 0) AS UnavailableListings,
                COALESCE(SUM(CASE WHEN c.Status = N'Sold' THEN 1 ELSE 0 END), 0) AS SoldListings,
                (SELECT COUNT(*) FROM dbo.CarInquiries AS i
                 INNER JOIN dbo.CarListings AS ci ON ci.Id = i.CarListingId
                 WHERE ci.SellerId = @SellerId) AS TotalInquiries,
                (SELECT COUNT(*) FROM dbo.CarInquiries AS i
                 INNER JOIN dbo.CarListings AS ci ON ci.Id = i.CarListingId
                 WHERE ci.SellerId = @SellerId
                   AND i.Status = N'Pending') AS PendingInquiries
            FROM dbo.CarListings AS c
            WHERE c.SellerId = @SellerId;
            """, new { SellerId = sellerId }, cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<SellerDashboardStats>(command);
    }
}
