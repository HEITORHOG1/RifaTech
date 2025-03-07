using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class DrawService : IDrawService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DrawService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DrawDTO> CreateDrawAsync(string rifaId, DrawDTO drawDto)
        {
            if (!Guid.TryParse(rifaId, out Guid guidRifaId))
            {
                throw new Exception("RifaId inválido.");
            }

            var rifa = await _context.Rifas.FindAsync(guidRifaId);
            if (rifa == null)
            {
                throw new Exception("Rifa não encontrada.");
            }

            var draw = _mapper.Map<Draw>(drawDto);
            draw.RifaId = guidRifaId; // Use o Guid convertido aqui

            _context.Draws.Add(draw);
            await _context.SaveChangesAsync();

            return _mapper.Map<DrawDTO>(draw);
        }

        public async Task<DrawDTO> ExecuteDrawAsync(string id)
        {
            var draw = await _context.Draws.FindAsync(id);
            if (draw == null)
            {
                throw new Exception("Sorteio não encontrado.");
            }

            // Aqui você pode adicionar a lógica para executar o sorteio
            // Por exemplo, determinar o número vencedor

            await _context.SaveChangesAsync();
            return _mapper.Map<DrawDTO>(draw);
        }

        public async Task<DrawDTO> GetDrawByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid guidId))
            {
                throw new Exception("Id inválido.");
            }

            var draw = await _context.Draws.Include(d => d.Rifa).FirstOrDefaultAsync(d => d.Id == guidId);
            return draw != null ? _mapper.Map<DrawDTO>(draw) : null;
        }

        public async Task<IEnumerable<DrawDTO>> GetAllDrawsAsync()
        {
            var draws = await _context.Draws.Include(d => d.Rifa).ToListAsync();
            return _mapper.Map<IEnumerable<DrawDTO>>(draws);
        }
    }
}