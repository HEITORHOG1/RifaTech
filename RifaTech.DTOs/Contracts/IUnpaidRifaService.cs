using RifaTech.DTOs.DTOs;

namespace RifaTech.DTOs.Contracts
{
    public interface IUnpaidRifaService
    {
        Task<IEnumerable<UnpaidRifaDTO>> GetAllUnpaidRifasAsync();

        Task<UnpaidRifaDTO> CreateUnpaidRifaAsync(UnpaidRifaDTO unpaidRifaDto);

        Task<IEnumerable<UnpaidRifaDTO>> GetUnpaidRifasByClienteIdAsync(Guid clienteId);
    }
}