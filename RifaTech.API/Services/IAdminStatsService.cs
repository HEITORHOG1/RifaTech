using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Services
{
    public interface IAdminStatsService
    {
        Task<AdminDashboardStatsDTO> GetDashboardStatsAsync();
        Task<SalesReportDTO> GetSalesReportAsync(DateTime? startDate, DateTime? endDate);
        Task<List<TopSellingRifaDTO>> GetTopSellingRifasAsync(int count = 5);
        Task<List<RecentTicketSaleDTO>> GetRecentTicketSalesAsync(int count = 10);
        Task<List<UpcomingDrawDTO>> GetUpcomingDrawsAsync(int count = 5);
    }
}
