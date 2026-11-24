using AutoVerifyMVC.Models;

namespace AutoVerifyMVC.Data.Repositories;

public interface IInquiryRepository
{
    Task<IReadOnlyList<InquiryViewModel>> GetBuyerInquiriesAsync(
        string buyerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InquiryViewModel>> GetSellerInquiriesAsync(
        string sellerId, CancellationToken cancellationToken = default);

    Task<InspectorDashboardViewModel> GetInspectorDashboardAsync(
        string inspectorId, CancellationToken cancellationToken = default);

    Task<bool> ClaimInspectionAsync(
        int inquiryId, string inspectorId, CancellationToken cancellationToken = default);

    Task<bool> SubmitInspectionReportAsync(
        InspectionReportInputModel report, string inspectorId, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(CarInquiry inquiry, CancellationToken cancellationToken = default);
}
