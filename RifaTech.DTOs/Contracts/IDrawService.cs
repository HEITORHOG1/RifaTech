using RifaTech.DTOs.DTOs;

namespace RifaTech.DTOs.Contracts
{
    public interface IDrawService
    {
        Task<DrawDTO> CreateDrawAsync(string rifaId, DrawDTO draw);

        Task<DrawDTO> ExecuteDrawAsync(string id);

        Task<DrawDTO> GetDrawByIdAsync(string id);

        Task<IEnumerable<DrawDTO>> GetAllDrawsAsync();
    }
}