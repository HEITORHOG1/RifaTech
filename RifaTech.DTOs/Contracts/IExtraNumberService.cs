using RifaTech.DTOs.DTOs;

namespace RifaTech.DTOs.Contracts
{
    public interface IExtraNumberService
    {
        Task<ExtraNumberDTO> AddExtraNumberAsync(string rifaId, ExtraNumberDTO extraNumber);

        Task<ExtraNumberDTO> UpdateExtraNumberAsync(string id, ExtraNumberDTO extraNumber);

        Task DeleteExtraNumberAsync(string id);

        Task<IEnumerable<ExtraNumberDTO>> GetExtraNumbersByRifaAsync(string rifaId);
    }
}