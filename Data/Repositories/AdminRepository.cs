using AutoVerifyMVC.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace AutoVerifyMVC.Data.Repositories;

public class AdminRepository(ApplicationDbContext dbContext) : IAdminRepository
{
    public async Task<IReadOnlyList<AdminUserRowViewModel>> GetUsersAsync(
        string? userSearch = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition("""
            SELECT TOP (100) u.Id, u.FullName, u.Email,
                   CAST(CASE WHEN u.LockoutEnd > SYSUTCDATETIME() THEN 1 ELSE 0 END AS bit) AS IsSuspended,
                   COALESCE(roleSet.Roles, N'No role') AS Roles
            FROM dbo.AspNetUsers AS u
            OUTER APPLY (
                SELECT STRING_AGG(r.Name, N', ') AS Roles
                FROM dbo.AspNetUserRoles AS ur
                INNER JOIN dbo.AspNetRoles AS r ON r.Id = ur.RoleId
                WHERE ur.UserId = u.Id
            ) AS roleSet
            WHERE (@UserSearch IS NULL OR u.FullName LIKE N'%' + @UserSearch + N'%' OR u.Email LIKE N'%' + @UserSearch + N'%')
            ORDER BY u.FullName, u.Email;
            """, new { UserSearch = string.IsNullOrWhiteSpace(userSearch) ? null : userSearch.Trim() },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<AdminUserRowViewModel>(command);
        return rows.AsList();
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync(
        string? userSearch = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition("""
            SELECT
                (SELECT COUNT(*) FROM dbo.AspNetUsers) AS TotalUsers,
                (SELECT COUNT(DISTINCT ur.UserId) FROM dbo.AspNetUserRoles ur INNER JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE r.NormalizedName=N'BUYER') AS BuyerCount,
                (SELECT COUNT(DISTINCT ur.UserId) FROM dbo.AspNetUserRoles ur INNER JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE r.NormalizedName=N'SELLER') AS SellerCount,
                (SELECT COUNT(DISTINCT ur.UserId) FROM dbo.AspNetUserRoles ur INNER JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE r.NormalizedName=N'INSPECTOR') AS InspectorCount,
                (SELECT COUNT(DISTINCT ur.UserId) FROM dbo.AspNetUserRoles ur INNER JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE r.NormalizedName=N'ADMINISTRATOR') AS AdministratorCount,
                (SELECT COUNT(*) FROM dbo.CarListings) AS TotalListings,
                (SELECT COUNT(*) FROM dbo.CarListings WHERE Status=N'Available') AS AvailableListings,
                (SELECT COUNT(*) FROM dbo.CarInquiries) AS TotalInquiries,
                (SELECT COUNT(*) FROM dbo.CarInquiries WHERE Status=N'Pending') AS PendingInquiries,
                (SELECT COUNT(*) FROM dbo.CarInquiries WHERE Status=N'In Progress') AS ActiveInspections,
                (SELECT COUNT(*) FROM dbo.CarInquiries WHERE Status=N'Completed') AS CompletedInspections;

            SELECT TOP (100) u.Id, u.FullName, u.Email,
                   CAST(CASE WHEN u.LockoutEnd > SYSUTCDATETIME() THEN 1 ELSE 0 END AS bit) AS IsSuspended,
                   COALESCE(roleSet.Roles, N'No role') AS Roles
            FROM dbo.AspNetUsers AS u
            OUTER APPLY (
                SELECT STRING_AGG(r.Name, N', ') AS Roles
                FROM dbo.AspNetUserRoles AS ur
                INNER JOIN dbo.AspNetRoles AS r ON r.Id = ur.RoleId
                WHERE ur.UserId = u.Id
            ) AS roleSet
            WHERE (@UserSearch IS NULL OR u.FullName LIKE N'%' + @UserSearch + N'%' OR u.Email LIKE N'%' + @UserSearch + N'%')
            ORDER BY u.FullName, u.Email;

            SELECT TOP (30) i.Id,
                CONCAT(c.ModelYear, N' ', c.Make, N' ', c.Model, N' ', c.Trim) AS VehicleName,
                COALESCE(b.FullName, b.Email, N'Buyer') AS BuyerName,
                COALESCE(s.FullName, s.Email, N'Platform inventory') AS SellerName,
                COALESCE(inspector.FullName, N'Unassigned') AS InspectorName,
                i.Status, i.CreatedAtUtc
            FROM dbo.CarInquiries AS i
            INNER JOIN dbo.CarListings AS c ON c.Id = i.CarListingId
            INNER JOIN dbo.AspNetUsers AS b ON b.Id = i.BuyerId
            LEFT JOIN dbo.AspNetUsers AS s ON s.Id = c.SellerId
            LEFT JOIN dbo.AspNetUsers AS inspector ON inspector.Id = i.InspectorId
            ORDER BY i.CreatedAtUtc DESC;

            SELECT TOP (15) c.Id,
                   CONCAT(c.ModelYear, N' ', c.Make, N' ', c.Model, N' ', c.Trim) AS VehicleName,
                   COALESCE(s.FullName, s.Email, N'Platform inventory') AS SellerName,
                   c.Price, c.Status, c.ImagePath, c.CreatedAtUtc
            FROM dbo.CarListings AS c
            LEFT JOIN dbo.AspNetUsers AS s ON s.Id = c.SellerId
            ORDER BY c.CreatedAtUtc DESC;
            """, new { UserSearch = string.IsNullOrWhiteSpace(userSearch) ? null : userSearch.Trim() },
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var model = await results.ReadSingleAsync<AdminDashboardViewModel>();
        model.Users = (await results.ReadAsync<AdminUserRowViewModel>()).AsList();
        model.Inquiries = (await results.ReadAsync<AdminInquiryRowViewModel>()).AsList();
        model.Listings = (await results.ReadAsync<AdminListingRowViewModel>()).AsList();
        model.UserSearch = userSearch?.Trim() ?? string.Empty;
        return model;
    }

    public async Task<bool> SetListingRemovedAsync(int listingId, bool removed, CancellationToken cancellationToken = default)
    {
        var listing = await dbContext.CarListings.FirstOrDefaultAsync(car => car.Id == listingId, cancellationToken);
        if (listing is null)
            return false;

        if (removed)
        {
            if (listing.Status == "Removed") return false;
            listing.Status = "Removed";
        }
        else
        {
            if (listing.Status != "Removed") return false;
            // Restore conservatively; a seller can set it back to Available after review.
            listing.Status = "Unavailable";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetInquiryCancelledAsync(int inquiryId, bool cancelled, CancellationToken cancellationToken = default)
    {
        var inquiry = await dbContext.CarInquiries.FirstOrDefaultAsync(item => item.Id == inquiryId, cancellationToken);
        if (inquiry is null)
            return false;

        if (cancelled)
        {
            if (inquiry.Status is "Completed" or "Cancelled") return false;
            inquiry.Status = "Cancelled";
            inquiry.InspectorId = null;
        }
        else
        {
            if (inquiry.Status != "Cancelled") return false;
            inquiry.Status = "Pending";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
