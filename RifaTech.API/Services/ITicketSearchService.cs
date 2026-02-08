using RifaTech.DTOs.DTOs;
using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Services
{
    public interface ITicketSearchService
    {
        Task<TicketSearchResultDTO> SearchTicketsAsync(TicketSearchQueryDTO query);

        Task<TicketSummaryDTO> GetTicketSummaryByRifaAsync(Guid rifaId);

        Task<List<TicketDTO>> GetTicketsByNumbersAsync(Guid rifaId, List<int> numbers);

        Task<List<TicketDTO>> GetTicketsByClienteAsync(Guid clienteId, int pageNumber = 1, int pageSize = 20);
    }
}