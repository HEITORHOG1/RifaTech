using RifaTech.DTOs.DTOs;

namespace RifaTech.DTOs.Contracts
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteDTO>> GetAllClientesAsync();

        Task<ClienteDTO?> GetClienteByIdAsync(Guid id);

        Task<ClienteDTO> CreateClienteAsync(ClienteDTO clienteDto);

        Task<bool> UpdateClienteAsync(Guid id, ClienteDTO cliente);

        Task<bool> DeleteClienteAsync(Guid id);

        //criar GetClienteByEmailOrPhoneNumberOrCPFAsync
        Task<ClienteDTO?> GetClienteByEmailOrPhoneNumberOrCPFAsync(string email, string phoneNumber, string cpf);
    }
}