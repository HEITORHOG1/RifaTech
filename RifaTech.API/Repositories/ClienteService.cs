using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClienteService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ClienteDTO>> GetAllClientesAsync()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return _mapper.Map<IEnumerable<ClienteDTO>>(clientes);
        }

        public async Task<ClienteDTO?> GetClienteByIdAsync(Guid id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            return cliente != null ? _mapper.Map<ClienteDTO>(cliente) : null;
        }

        public async Task<ClienteDTO> CreateClienteAsync(ClienteDTO clienteDto)
        {
            var cliente = _mapper.Map<Cliente>(clienteDto);

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return _mapper.Map<ClienteDTO>(cliente);
        }

        public async Task<bool> UpdateClienteAsync(Guid id, ClienteDTO updatedClienteDto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return false;
            cliente.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(updatedClienteDto, cliente);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClienteAsync(Guid id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return false;

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return true;
        }

        //criar GetClienteByEmailOrPhoneNumberOrCPFAsync
        public async Task<ClienteDTO?> GetClienteByEmailOrPhoneNumberOrCPFAsync(string email, string phoneNumber, string cpf)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email || c.PhoneNumber == phoneNumber || c.CPF == cpf);
            return cliente != null ? _mapper.Map<ClienteDTO>(cliente) : null;
        }
    }
}