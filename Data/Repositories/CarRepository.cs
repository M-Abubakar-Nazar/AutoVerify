using AutoVerifyMVC.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace AutoVerifyMVC.Data.Repositories;

public class CarRepository(ApplicationDbContext dbContext) : ICarRepository
{
    private const string CardQuery = """
        SELECT c.Id, c.Make, c.Model, c.Trim, c.ModelYear, c.Price, c.MileageKm,
               c.Engine, c.Transmission, c.FuelType, c.Description, c.ImagePath,
               c.Status, c.IsSeedData, c.SellerId, COALESCE(u.FullName, N'AutoVerify') AS SellerName
        FROM dbo.CarListings AS c
        LEFT JOIN dbo.AspNetUsers AS u ON u.Id = c.SellerId
        """;

    public async Task<CarListingPageViewModel> GetPublicListingsAsync(
        string? searchTerm, string? priceRange, int skip, int take, CancellationToken cancellationToken = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 100);
        var priceBounds = CarPriceRanges.GetBounds(priceRange);
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition(
            CardQuery + Environment.NewLine + """
            WHERE c.Status <> N'Removed'
              AND (@SearchTerm IS NULL OR c.Make LIKE @Pattern OR c.Model LIKE @Pattern OR c.Trim LIKE @Pattern)
              AND (@MinimumPrice IS NULL OR (@MinimumInclusive = 1 AND c.Price >= @MinimumPrice) OR (@MinimumInclusive = 0 AND c.Price > @MinimumPrice))
              AND (@MaximumPrice IS NULL OR (@MaximumInclusive = 1 AND c.Price <= @MaximumPrice) OR (@MaximumInclusive = 0 AND c.Price < @MaximumPrice))
            ORDER BY c.CreatedAtUtc DESC, c.Id
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY
            """,
            new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim(),
                Pattern = $"%{searchTerm?.Trim()}%",
                MinimumPrice = priceBounds.MinimumPrice,
                MaximumPrice = priceBounds.MaximumPrice,
                MinimumInclusive = priceBounds.MinimumInclusive,
                MaximumInclusive = priceBounds.MaximumInclusive,
                Skip = skip,
                PageSize = take + 1
            },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<CarListingViewModel>(command);
        var result = rows.AsList();
        var hasMore = result.Count > take;
        if (hasMore)
            result.RemoveAt(result.Count - 1);
        return new CarListingPageViewModel { Cars = result, HasMore = hasMore };
    }

    public async Task<IReadOnlyList<CarStatusViewModel>> GetPublicStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition(
            "SELECT Id, Status, SellerId FROM dbo.CarListings WHERE Status <> N'Removed'",
            cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<CarStatusViewModel>(command);
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CarListingViewModel>> GetSellerListingsAsync(
        string sellerId, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition(
            CardQuery + Environment.NewLine + """
            WHERE c.SellerId = @SellerId
            ORDER BY c.CreatedAtUtc DESC, c.Id
            """,
            new { SellerId = sellerId }, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<CarListingViewModel>(command);
        return rows.AsList();
    }

    public Task<CarListing?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.CarListings.AsNoTracking().FirstOrDefaultAsync(car => car.Id == id, cancellationToken);

    public async Task<bool> UpdateStatusAsync(
        int id, string sellerId, string status, CancellationToken cancellationToken = default)
    {
        var listing = await dbContext.CarListings.FirstOrDefaultAsync(
            car => car.Id == id && car.SellerId == sellerId && car.Status != "Removed", cancellationToken);
        if (listing is null)
            return false;

        listing.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CreateAsync(CarListing listing, CancellationToken cancellationToken = default)
    {
        dbContext.CarListings.Add(listing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return listing.Id;
    }
}
