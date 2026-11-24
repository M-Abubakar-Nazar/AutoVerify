using AutoVerifyMVC.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace AutoVerifyMVC.Data.Repositories;

public class InquiryRepository(ApplicationDbContext dbContext) : IInquiryRepository
{
    private const string InquiryQuery = """
        SELECT i.Id, i.CarListingId,
               CONCAT(c.ModelYear, N' ', c.Make, N' ', c.Model, N' ', c.Trim) AS VehicleName,
               COALESCE(b.FullName, b.Email, N'Buyer') AS BuyerName,
               i.Message, i.Status, i.CreatedAtUtc,
               COALESCE(inspector.FullName, N'Unassigned') AS InspectorName,
               i.InspectorId, i.EngineCondition, i.TransmissionCondition,
               i.BodyCondition, i.BrakesCondition, i.InspectionNotes, i.CompletedAtUtc,
               c.ImagePath
        FROM dbo.CarInquiries AS i
        INNER JOIN dbo.CarListings AS c ON c.Id = i.CarListingId
        INNER JOIN dbo.AspNetUsers AS b ON b.Id = i.BuyerId
        LEFT JOIN dbo.AspNetUsers AS inspector ON inspector.Id = i.InspectorId
        """;

    public async Task<IReadOnlyList<InquiryViewModel>> GetBuyerInquiriesAsync(
        string buyerId, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition(
            InquiryQuery + Environment.NewLine + "WHERE i.BuyerId = @BuyerId ORDER BY i.CreatedAtUtc DESC",
            new { BuyerId = buyerId }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<InquiryViewModel>(command);
        return rows.AsList();
    }

    public async Task<IReadOnlyList<InquiryViewModel>> GetSellerInquiriesAsync(
        string sellerId, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition(
            InquiryQuery + Environment.NewLine + """
            WHERE c.SellerId = @SellerId
            ORDER BY i.CreatedAtUtc DESC
            """,
            new { SellerId = sellerId }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<InquiryViewModel>(command);
        return rows.AsList();
    }

    public async Task<InspectorDashboardViewModel> GetInspectorDashboardAsync(
        string inspectorId, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var command = new CommandDefinition("""
            SELECT
                (SELECT COUNT(*) FROM dbo.CarInquiries WHERE Status = N'Pending' AND InspectorId IS NULL) AS AvailableInspections,
                (SELECT COUNT(*) FROM dbo.CarInquiries WHERE InspectorId = @InspectorId AND Status = N'In Progress') AS ActiveInspections,
                (SELECT COUNT(*) FROM dbo.CarInquiries WHERE InspectorId = @InspectorId AND Status = N'Completed') AS CompletedInspections;

            SELECT i.Id, i.CarListingId,
                   CONCAT(c.ModelYear, N' ', c.Make, N' ', c.Model, N' ', c.Trim) AS VehicleName,
                   COALESCE(b.FullName, b.Email, N'Buyer') AS BuyerName,
                   COALESCE(s.FullName, s.Email, N'Seller') AS SellerName,
                   i.Message, i.Status, i.CreatedAtUtc, i.InspectorId,
                   COALESCE(inspector.FullName, N'Unassigned') AS InspectorName,
                   i.EngineCondition, i.TransmissionCondition, i.BodyCondition,
                   i.BrakesCondition, i.InspectionNotes, i.CompletedAtUtc, c.ImagePath
            FROM dbo.CarInquiries AS i
            INNER JOIN dbo.CarListings AS c ON c.Id = i.CarListingId
            INNER JOIN dbo.AspNetUsers AS b ON b.Id = i.BuyerId
            LEFT JOIN dbo.AspNetUsers AS s ON s.Id = c.SellerId
            LEFT JOIN dbo.AspNetUsers AS inspector ON inspector.Id = i.InspectorId
            WHERE i.Status = N'Pending' AND i.InspectorId IS NULL
            ORDER BY i.CreatedAtUtc;

            SELECT i.Id, i.CarListingId,
                   CONCAT(c.ModelYear, N' ', c.Make, N' ', c.Model, N' ', c.Trim) AS VehicleName,
                   COALESCE(b.FullName, b.Email, N'Buyer') AS BuyerName,
                   COALESCE(s.FullName, s.Email, N'Seller') AS SellerName,
                   i.Message, i.Status, i.CreatedAtUtc, i.InspectorId,
                   COALESCE(inspector.FullName, N'Unassigned') AS InspectorName,
                   i.EngineCondition, i.TransmissionCondition, i.BodyCondition,
                   i.BrakesCondition, i.InspectionNotes, i.CompletedAtUtc, c.ImagePath
            FROM dbo.CarInquiries AS i
            INNER JOIN dbo.CarListings AS c ON c.Id = i.CarListingId
            INNER JOIN dbo.AspNetUsers AS b ON b.Id = i.BuyerId
            LEFT JOIN dbo.AspNetUsers AS s ON s.Id = c.SellerId
            LEFT JOIN dbo.AspNetUsers AS inspector ON inspector.Id = i.InspectorId
            WHERE i.InspectorId = @InspectorId
            ORDER BY CASE WHEN i.Status = N'In Progress' THEN 0 ELSE 1 END, i.CreatedAtUtc DESC;
            """, new { InspectorId = inspectorId }, cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var stats = await results.ReadSingleAsync<InspectorDashboardStats>();
        var queue = (await results.ReadAsync<InquiryViewModel>()).AsList();
        var assigned = (await results.ReadAsync<InquiryViewModel>()).AsList();
        return new InspectorDashboardViewModel
        {
            AvailableInspections = stats.AvailableInspections,
            ActiveInspections = stats.ActiveInspections,
            CompletedInspections = stats.CompletedInspections,
            Queue = queue,
            AssignedInspections = assigned
        };
    }

    public async Task<bool> ClaimInspectionAsync(
        int inquiryId, string inspectorId, CancellationToken cancellationToken = default)
    {
        var affected = await dbContext.CarInquiries
            .Where(inquiry => inquiry.Id == inquiryId
                && inquiry.Status == "Pending"
                && inquiry.InspectorId == null)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(inquiry => inquiry.InspectorId, inspectorId)
                .SetProperty(inquiry => inquiry.Status, "In Progress"), cancellationToken);
        return affected == 1;
    }

    public async Task<bool> SubmitInspectionReportAsync(
        InspectionReportInputModel report, string inspectorId, CancellationToken cancellationToken = default)
    {
        var inquiry = await dbContext.CarInquiries.FirstOrDefaultAsync(
            item => item.Id == report.InquiryId
                && item.InspectorId == inspectorId
                && item.Status == "In Progress", cancellationToken);
        if (inquiry is null)
            return false;

        inquiry.EngineCondition = report.EngineCondition;
        inquiry.TransmissionCondition = report.TransmissionCondition;
        inquiry.BodyCondition = report.BodyCondition;
        inquiry.BrakesCondition = report.BrakesCondition;
        inquiry.InspectionNotes = report.InspectionNotes?.Trim();
        inquiry.Status = "Completed";
        inquiry.CompletedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CreateAsync(CarInquiry inquiry, CancellationToken cancellationToken = default)
    {
        dbContext.CarInquiries.Add(inquiry);
        await dbContext.SaveChangesAsync(cancellationToken);
        return inquiry.Id;
    }
}
