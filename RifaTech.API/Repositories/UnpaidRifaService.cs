using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class UnpaidRifaService : IUnpaidRifaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UnpaidRifaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UnpaidRifaDTO>> GetAllUnpaidRifasAsync()
        {
            var unpaidRifas = await _context.UnpaidRifas.ToListAsync();
            return _mapper.Map<IEnumerable<UnpaidRifaDTO>>(unpaidRifas);
        }

        public async Task<UnpaidRifaDTO> CreateUnpaidRifaAsync(UnpaidRifaDTO unpaidRifaDto)
        {
            var unpaidRifa = _mapper.Map<UnpaidRifa>(unpaidRifaDto);
            _context.UnpaidRifas.Add(unpaidRifa);
            await _context.SaveChangesAsync();
            return _mapper.Map<UnpaidRifaDTO>(unpaidRifa);
        }

        public async Task<IEnumerable<UnpaidRifaDTO>> GetUnpaidRifasByClienteIdAsync(Guid clienteId)
        {
            var unpaidRifas = await _context.UnpaidRifas
                .Include(ur => ur.Cliente)
                .Include(ur => ur.Rifa)
                .Where(ur => ur.ClienteId == clienteId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UnpaidRifaDTO>>(unpaidRifas);
        }
    }
}