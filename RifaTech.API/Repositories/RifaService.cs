using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.API.Services;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class RifaService : IRifaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RifaService> _logger;

        // Cache keys
        private const string ALL_RIFAS_CACHE_KEY = "all_rifas";
        private const string FEATURED_RIFAS_CACHE_KEY = "featured_rifas";
        private const string RIFA_BY_ID_CACHE_KEY_PREFIX = "rifa_";

        public RifaService(
            AppDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<RifaService> logger)
        {
            _context = context;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<RifaDTO> CreateRifaAsync(RifaDTO rifaDto)
        {
            var rifa = _mapper.Map<Rifa>(rifaDto);

            _context.Rifas.Add(rifa);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cacheService.Remove(ALL_RIFAS_CACHE_KEY);
            _cacheService.Remove(FEATURED_RIFAS_CACHE_KEY);

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

            // Invalidate cache
            _cacheService.Remove(ALL_RIFAS_CACHE_KEY);
            _cacheService.Remove(FEATURED_RIFAS_CACHE_KEY);
            _cacheService.Remove($"{RIFA_BY_ID_CACHE_KEY_PREFIX}{id}");

            return _mapper.Map<RifaDTO>(existingRifa);
        }

        public async Task<RifaDTO> GetRifaByIdAsync(Guid id)
        {
            string cacheKey = $"{RIFA_BY_ID_CACHE_KEY_PREFIX}{id}";

            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation($"Cache miss for rifa ID {id}, loading from database");

                // Carrega a rifa sem rastreamento de alterações
                var rifa = await _context.Rifas
                    .Where(x => x.EhDeleted == false && x.Id == id)
                    .AsNoTracking()
                    .Include(r => r.Tickets)
                        .ThenInclude(t => t.Cliente)
                    .Include(r => r.ExtraNumbers)
                    .FirstOrDefaultAsync();

                return rifa != null ? _mapper.Map<RifaDTO>(rifa) : null;
            });
        }

        public async Task<IEnumerable<RifaDTO>> GetAllRifasAsync()
        {
            return await _cacheService.GetOrCreateAsync(ALL_RIFAS_CACHE_KEY, async () =>
            {
                _logger.LogInformation("Cache miss for all rifas, loading from database");

                var rifas = await _context.Rifas
                    .Where(x => x.EhDeleted == false)
                    .Include(r => r.Tickets)
                    .AsNoTracking()
                    .ToListAsync();

                // Add extra calculated properties for UI
                var rifaDTOs = _mapper.Map<IEnumerable<RifaDTO>>(rifas);
                foreach (var rifaDTO in rifaDTOs)
                {
                    // Calculate progress percentage
                    if (rifaDTO.MaxTickets > 0 && rifaDTO.Tickets != null)
                    {
                        rifaDTO.ProgressPercentage = (rifaDTO.Tickets.Count * 100) / rifaDTO.MaxTickets;
                    }

                    // Calculate time remaining for the draw
                    rifaDTO.TimeRemaining = rifaDTO.DrawDateTime > DateTime.UtcNow
                        ? (rifaDTO.DrawDateTime - DateTime.UtcNow).ToString(@"dd\d\ hh\h\ mm\m")
                        : "Encerrado";
                }

                return rifaDTOs;
            });
        }

        public async Task<IEnumerable<RifaDTO>> GetFeaturedRifasAsync()
        {
            return await _cacheService.GetOrCreateAsync(FEATURED_RIFAS_CACHE_KEY, async () =>
            {
                _logger.LogInformation("Cache miss for featured rifas, loading from database");

                // Get rifas that are:
                // 1. Not deleted
                // 2. Have a DrawDateTime in the future
                // 3. Order by closest DrawDateTime first
                // 4. Limit to 5 for featured display
                var rifas = await _context.Rifas
                    .Where(r => r.EhDeleted == false)
                    .Where(r => r.DrawDateTime > DateTime.UtcNow)
                    .OrderBy(r => r.DrawDateTime)
                    .Include(r => r.Tickets)
                    .AsNoTracking()
                    .Take(5)
                    .ToListAsync();

                // Add extra calculated properties for UI
                var rifaDTOs = _mapper.Map<IEnumerable<RifaDTO>>(rifas);
                foreach (var rifaDTO in rifaDTOs)
                {
                    // Calculate progress percentage
                    if (rifaDTO.MaxTickets > 0 && rifaDTO.Tickets != null)
                    {
                        rifaDTO.ProgressPercentage = (rifaDTO.Tickets.Count * 100) / rifaDTO.MaxTickets;
                    }

                    // Calculate time remaining for the draw
                    rifaDTO.TimeRemaining = rifaDTO.DrawDateTime > DateTime.UtcNow
                        ? (rifaDTO.DrawDateTime - DateTime.UtcNow).ToString(@"dd\d\ hh\h\ mm\m")
                        : "Encerrado";
                }

                return rifaDTOs;
            });
        }

        public async Task DeleteRifaAsync(string id)
        {
            var rifa = await _context.Rifas.FindAsync(id);
            if (rifa != null)
            {
                _context.Rifas.Remove(rifa);
                await _context.SaveChangesAsync();

                // Invalidate cache
                _cacheService.Remove(ALL_RIFAS_CACHE_KEY);
                _cacheService.Remove(FEATURED_RIFAS_CACHE_KEY);
                _cacheService.Remove($"{RIFA_BY_ID_CACHE_KEY_PREFIX}{id}");
            }
        }

        public async Task<(IEnumerable<RifaDTO> Rifas, int TotalCount)> GetRifasPaginatedAsync(int pageNumber, int pageSize)
        {
            // For pagination, we don't use cache as the results will frequently change

            // Calcula o número total de rifas
            var totalCount = await _context.Rifas
                .Where(r => r.EhDeleted == false)
                .CountAsync();

            // Busca um subconjunto de rifas com base na paginação
            var rifas = await _context.Rifas
                .Where(r => r.EhDeleted == false)
                .Include(r => r.Tickets)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mapeia as entidades para DTOs e adiciona propriedades calculadas
            var rifaDTOs = _mapper.Map<IEnumerable<RifaDTO>>(rifas);
            foreach (var rifaDTO in rifaDTOs)
            {
                // Calculate progress percentage
                if (rifaDTO.MaxTickets > 0 && rifaDTO.Tickets != null)
                {
                    rifaDTO.ProgressPercentage = (rifaDTO.Tickets.Count * 100) / rifaDTO.MaxTickets;
                }

                // Calculate time remaining for the draw
                rifaDTO.TimeRemaining = rifaDTO.DrawDateTime > DateTime.UtcNow
                    ? (rifaDTO.DrawDateTime - DateTime.UtcNow).ToString(@"dd\d\ hh\h\ mm\m")
                    : "Encerrado";
            }

            return (rifaDTOs, totalCount);
        }

        public async Task<(IEnumerable<RifaDTO> Rifas, int TotalCount)> GetRifasPaginatedAsync(int pageNumber, int pageSize, string searchTerm, DateTime? startDate, DateTime? endDate)
        {
            // For pagination with filters, we don't use cache

            // Constrói a consulta com base nos critérios de pesquisa
            var query = _context.Rifas
                .Where(r => r.EhDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm) || r.Description.Contains(searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.EndDate <= endDate.Value);
            }

            // Calcula o número total de rifas após a aplicação dos filtros
            var totalCount = await query.CountAsync();

            // Busca um subconjunto de rifas com base na paginação
            var rifas = await query
                .Include(r => r.Tickets)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mapeia as entidades para DTOs e adiciona propriedades calculadas
            var rifaDTOs = _mapper.Map<IEnumerable<RifaDTO>>(rifas);
            foreach (var rifaDTO in rifaDTOs)
            {
                // Calculate progress percentage
                if (rifaDTO.MaxTickets > 0 && rifaDTO.Tickets != null)
                {
                    rifaDTO.ProgressPercentage = (rifaDTO.Tickets.Count * 100) / rifaDTO.MaxTickets;
                }

                // Calculate time remaining for the draw
                rifaDTO.TimeRemaining = rifaDTO.DrawDateTime > DateTime.UtcNow
                    ? (rifaDTO.DrawDateTime - DateTime.UtcNow).ToString(@"dd\d\ hh\h\ mm\m")
                    : "Encerrado";
            }

            return (rifaDTOs, totalCount);
        }

        public async Task MarkRifaAsDeletedAsync(Guid id)
        {
            var rifa = await _context.Rifas.FindAsync(id);
            if (rifa != null)
            {
                rifa.EhDeleted = true;
                rifa.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Invalidate cache
                _cacheService.Remove(ALL_RIFAS_CACHE_KEY);
                _cacheService.Remove(FEATURED_RIFAS_CACHE_KEY);
                _cacheService.Remove($"{RIFA_BY_ID_CACHE_KEY_PREFIX}{id}");
            }
            else
            {
                throw new KeyNotFoundException($"Rifa with ID {id} not found");
            }
        }
    }
}