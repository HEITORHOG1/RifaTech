using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class ExtraNumberService : IExtraNumberService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ExtraNumberService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ExtraNumberDTO> AddExtraNumberAsync(string rifaId, ExtraNumberDTO extraNumberDto)
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

            var extraNumber = _mapper.Map<ExtraNumber>(extraNumberDto);
            extraNumber.RifaId = guidRifaId; // Use o Guid convertido aqui

            _context.ExtraNumbers.Add(extraNumber);
            await _context.SaveChangesAsync();

            return _mapper.Map<ExtraNumberDTO>(extraNumber);
        }

        public async Task<ExtraNumberDTO> UpdateExtraNumberAsync(string id, ExtraNumberDTO extraNumberDto)
        {
            var existingExtraNumber = await _context.ExtraNumbers.FindAsync(id);
            if (existingExtraNumber == null)
            {
                return null;
            }
            existingExtraNumber.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(extraNumberDto, existingExtraNumber);
            _context.ExtraNumbers.Update(existingExtraNumber);
            await _context.SaveChangesAsync();

            return _mapper.Map<ExtraNumberDTO>(existingExtraNumber);
        }

        public async Task DeleteExtraNumberAsync(string id)
        {
            var extraNumber = await _context.ExtraNumbers.FindAsync(id);
            if (extraNumber != null)
            {
                _context.ExtraNumbers.Remove(extraNumber);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ExtraNumberDTO>> GetExtraNumbersByRifaAsync(string rifaId)
        {
            Guid guidRifaId;
            if (!Guid.TryParse(rifaId, out guidRifaId))
            {
                // Lidar com o caso em que rifaId não é um Guid válido
            }

            var extraNumbers = await _context.ExtraNumbers
                .Where(en => en.RifaId == guidRifaId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ExtraNumberDTO>>(extraNumbers);
        }
    }
}