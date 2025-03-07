using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class RifaService : IRifaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RifaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RifaDTO> CreateRifaAsync(RifaDTO rifaDto)
        {
            var rifa = _mapper.Map<Rifa>(rifaDto);

            _context.Rifas.Add(rifa);
            await _context.SaveChangesAsync();

            return _mapper.Map<RifaDTO>(rifa);
        }

        public async Task<RifaDTO> UpdateRifaAsync(Guid id, RifaDTO rifaDto)
        {
            var existingRifa = await _context.Rifas.FindAsync(id);
            if (existingRifa == null)
            {
                return null;
            }
            existingRifa.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(rifaDto, existingRifa);
            _context.Rifas.Update(existingRifa);
            await _context.SaveChangesAsync();

            return _mapper.Map<RifaDTO>(existingRifa);
        }

        public async Task<RifaDTO> GetRifaByIdAsync(Guid id)
        {
            // Carrega a rifa sem rastreamento de alterações e inclui a propriedade ExtraNumbers
            var rifa = await _context.Rifas.Where(x => x.EhDeleted == false)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(r => r.Id == id);

            return rifa != null ? _mapper.Map<RifaDTO>(rifa) : null;
        }

        public async Task<IEnumerable<RifaDTO>> GetAllRifasAsync()
        {
            var rifas = await _context.Rifas.Where(x => x.EhDeleted == false).ToListAsync();
            return _mapper.Map<IEnumerable<RifaDTO>>(rifas);
        }

        public async Task DeleteRifaAsync(string id)
        {
            var rifa = await _context.Rifas.FindAsync(id);
            if (rifa != null)
            {
                _context.Rifas.Remove(rifa);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(IEnumerable<RifaDTO> Rifas, int TotalCount)> GetRifasPaginatedAsync(int pageNumber, int pageSize)
        {
            // Calcula o número total de rifas
            var totalCount = await _context.Rifas.CountAsync();

            // Busca um subconjunto de rifas com base na paginação
            var rifas = await _context.Rifas
                                      .Skip((pageNumber - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            // Mapeia as entidades para DTOs
            var rifasDto = _mapper.Map<IEnumerable<RifaDTO>>(rifas);

            return (rifasDto, totalCount);
        }

        public async Task<(IEnumerable<RifaDTO> Rifas, int TotalCount)> GetRifasPaginatedAsync(int pageNumber, int pageSize, string searchTerm, DateTime? startDate, DateTime? endDate)
        {
            // Constrói a consulta com base nos critérios de pesquisa
            var query = _context.Rifas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.EndDate <= endDate.Value);
            }
            query = query.Where(r => r.EhDeleted == false);

            // Calcula o número total de rifas após a aplicação dos filtros
            var totalCount = await query.CountAsync();

            // Busca um subconjunto de rifas com base na paginação
            var rifas = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            // Mapeia as entidades para DTOs
            var rifasDto = _mapper.Map<IEnumerable<RifaDTO>>(rifas);

            return (rifasDto, totalCount);
        }

        public async Task MarkRifaAsDeletedAsync(Guid id)
        {
            var rifa = await _context.Rifas.FindAsync(id);
            if (rifa != null)
            {
                rifa.EhDeleted = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Tratar caso em que a rifa não é encontrada (por exemplo, lançar uma exceção ou retornar um valor indicando falha)
            }
        }
    }
}