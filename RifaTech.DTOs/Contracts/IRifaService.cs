using RifaTech.DTOs.DTOs;

namespace RifaTech.DTOs.Contracts
{
    public interface IRifaService
    {
        Task<RifaDTO> CreateRifaAsync(RifaDTO rifa);

        Task<RifaDTO> UpdateRifaAsync(Guid id, RifaDTO rifa);

        Task<RifaDTO> GetRifaByIdAsync(Guid id);

        Task<IEnumerable<RifaDTO>> GetAllRifasAsync();

        Task<IEnumerable<RifaDTO>> GetFeaturedRifasAsync();

        Task DeleteRifaAsync(string id);

        Task<(IEnumerable<RifaDTO> Rifas, int TotalCount)> GetRifasPaginatedAsync(int pageNumber, int pageSize);

        Task<(IEnumerable<RifaDTO> Rifas, int TotalCount)> GetRifasPaginatedAsync(int pageNumber, int pageSize, string searchTerm, DateTime? startDate, DateTime? endDate);

        Task MarkRifaAsDeletedAsync(Guid id);
    }
}