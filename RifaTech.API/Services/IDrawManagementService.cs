using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Services
{
    public interface IDrawManagementService
    {
        Task<DrawResultDTO> ExecuteDrawAsync(Guid rifaId);
        Task<List<DrawResultDTO>> GetDrawHistoryAsync(int count = 10);
        Task<DrawPreviewDTO> GetDrawPreviewAsync(Guid rifaId);
        Task<bool> ScheduleDrawAsync(Guid rifaId, DateTime drawDateTime);
        Task<bool> CancelDrawAsync(Guid rifaId);
    }
}
