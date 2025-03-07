using RifaTech.DTOs.DTOs;

namespace RifaTech.DTOs.Contracts
{
    public interface ITicketService
    {
        Task<List<int>> PurchaseTicketAsync(string rifaId, TicketDTO ticketDto);

        Task<IEnumerable<TicketDTO>> GetTicketsByRifaAsync(Guid rifaId);

        Task<TicketDTO> GetTicketByIdAsync(string id);

        Task<IEnumerable<TicketDTO>> GetTicketsByUserAsync(Guid userId);

        Task CancelTicketAsync(string id);

        Task<TicketDTO> UpdateTicketAsync(string id, TicketDTO ticket);
    }
}