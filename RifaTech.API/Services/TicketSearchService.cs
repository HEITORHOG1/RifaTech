using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.DTOs;
using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Services
{
    public class TicketSearchService : ITicketSearchService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TicketSearchService> _logger;
        private readonly AutoMapper.IMapper _mapper;

        public TicketSearchService(
            AppDbContext context,
            ILogger<TicketSearchService> logger,
            AutoMapper.IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Pesquisa tickets com base em vários critérios
        /// </summary>
        public async Task<TicketSearchResultDTO> SearchTicketsAsync(TicketSearchQueryDTO query)
        {
            IQueryable<Ticket> ticketsQuery = _context.Tickets
                .Include(t => t.Rifa)
                .Include(t => t.Cliente);

            // Aplicar filtros
            if (query.RifaId.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.RifaId == query.RifaId.Value);
            }

            if (query.ClienteId.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.ClienteId == query.ClienteId.Value);
            }

            if (!string.IsNullOrEmpty(query.RifaName))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Rifa.Name.Contains(query.RifaName));
            }

            if (!string.IsNullOrEmpty(query.ClienteName))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Cliente.Name.Contains(query.ClienteName));
            }

            if (!string.IsNullOrEmpty(query.ClienteEmail))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Cliente.Email.Contains(query.ClienteEmail));
            }

            if (!string.IsNullOrEmpty(query.ClientePhone))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Cliente.PhoneNumber.Contains(query.ClientePhone));
            }

            if (query.TicketNumber.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.Number == query.TicketNumber.Value);
            }

            if (query.ValidOnly.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.EhValido == query.ValidOnly.Value);
            }

            if (query.CreatedAfter.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.CreatedAt >= query.CreatedAfter.Value);
            }

            if (query.CreatedBefore.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.CreatedAt <= query.CreatedBefore.Value);
            }

            // Contar o total de resultados antes de aplicar a paginação
            var totalCount = await ticketsQuery.CountAsync();

            // Aplicar ordenação
            if (query.OrderBy != null)
            {
                switch (query.OrderBy.ToLower())
                {
                    case "number":
                        ticketsQuery = query.OrderDirection == "desc"
                            ? ticketsQuery.OrderByDescending(t => t.Number)
                            : ticketsQuery.OrderBy(t => t.Number);
                        break;
                    case "createdAt":
                    case "date":
                        ticketsQuery = query.OrderDirection == "desc"
                            ? ticketsQuery.OrderByDescending(t => t.CreatedAt)
                            : ticketsQuery.OrderBy(t => t.CreatedAt);
                        break;
                    case "rifaName":
                        ticketsQuery = query.OrderDirection == "desc"
                            ? ticketsQuery.OrderByDescending(t => t.Rifa.Name)
                            : ticketsQuery.OrderBy(t => t.Rifa.Name);
                        break;
                    case "clienteName":
                        ticketsQuery = query.OrderDirection == "desc"
                            ? ticketsQuery.OrderByDescending(t => t.Cliente.Name)
                            : ticketsQuery.OrderBy(t => t.Cliente.Name);
                        break;
                    default:
                        ticketsQuery = ticketsQuery.OrderByDescending(t => t.CreatedAt);
                        break;
                }
            }
            else
            {
                // Ordenação padrão
                ticketsQuery = ticketsQuery.OrderByDescending(t => t.CreatedAt);
            }

            // Aplicar paginação
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 20;

            var tickets = await ticketsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var ticketDtos = _mapper.Map<List<TicketDTO>>(tickets);

            // Calcular páginas
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new TicketSearchResultDTO
            {
                Tickets = ticketDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }

        /// <summary>
        /// Obtém um resumo dos tickets para uma rifa específica
        /// </summary>
        public async Task<TicketSummaryDTO> GetTicketSummaryByRifaAsync(Guid rifaId)
        {
            var rifa = await _context.Rifas
                .Include(r => r.Tickets)
                .FirstOrDefaultAsync(r => r.Id == rifaId);

            if (rifa == null)
            {
                throw new KeyNotFoundException($"Rifa com ID {rifaId} não encontrada");
            }

            var tickets = rifa.Tickets;

            // Estatísticas gerais
            var totalTickets = tickets.Count;
            var validTickets = tickets.Count(t => t.EhValido);
            var pendingTickets = tickets.Count(t => !t.EhValido);

            // Top compradores
            var topBuyers = await _context.Tickets
                .Where(t => t.RifaId == rifaId && t.EhValido)
                .GroupBy(t => t.ClienteId)
                .Select(g => new TopBuyerDTO
                {
                    ClienteId = g.Key,
                    TicketCount = g.Count(),
                    Percentage = (double)g.Count() / Math.Max(1, validTickets) * 100
                })
                .OrderByDescending(b => b.TicketCount)
                .Take(5)
                .ToListAsync();

            // Buscar nomes dos clientes
            foreach (var buyer in topBuyers)
            {
                var cliente = await _context.Clientes.FindAsync(buyer.ClienteId);
                if (cliente != null)
                {
                    buyer.ClienteName = cliente.Name;
                    buyer.ClienteEmail = cliente.Email;
                }
            }

            // Distribuição dos números vendidos
            var numberRanges = new List<TicketRangeDTO>();
            if (rifa.MaxTickets > 0)
            {
                int rangeSize = rifa.MaxTickets <= 100 ? 10 : 100;

                for (int i = 0; i < rifa.MaxTickets; i += rangeSize)
                {
                    int rangeStart = i + 1;
                    int rangeEnd = Math.Min(i + rangeSize, rifa.MaxTickets);

                    var soldInRange = tickets
                        .Count(t => t.Number >= rangeStart && t.Number <= rangeEnd && t.EhValido);

                    var availableInRange = rangeEnd - rangeStart + 1 - soldInRange;

                    numberRanges.Add(new TicketRangeDTO
                    {
                        RangeStart = rangeStart,
                        RangeEnd = rangeEnd,
                        SoldCount = soldInRange,
                        AvailableCount = availableInRange,
                        Percentage = (double)soldInRange / (rangeEnd - rangeStart + 1) * 100
                    });
                }
            }

            // Vendas por dia (últimos 7 dias)
            var lastWeek = DateTime.UtcNow.AddDays(-7);
            var dailySales = await _context.Tickets
                .Where(t => t.RifaId == rifaId && t.CreatedAt >= lastWeek)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new DailySalesDTO
                {
                    Date = g.Key,
                    Count = g.Count(),
                    ValidCount = g.Count(t => t.EhValido)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            return new TicketSummaryDTO
            {
                RifaId = rifaId,
                RifaName = rifa.Name,
                TotalTickets = totalTickets,
                ValidTickets = validTickets,
                PendingTickets = pendingTickets,
                PercentageSold = rifa.MaxTickets > 0
                    ? (double)validTickets / rifa.MaxTickets * 100
                    : 0,
                TopBuyers = topBuyers,
                NumberRanges = numberRanges,
                DailySales = dailySales
            };
        }

        /// <summary>
        /// Busca tickets por números específicos em uma rifa
        /// </summary>
        public async Task<List<TicketDTO>> GetTicketsByNumbersAsync(Guid rifaId, List<int> numbers)
        {
            var tickets = await _context.Tickets
                .Include(t => t.Cliente)
                .Where(t => t.RifaId == rifaId && numbers.Contains(t.Number))
                .ToListAsync();

            return _mapper.Map<List<TicketDTO>>(tickets);
        }

        /// <summary>
        /// Busca tickets comprados por um cliente específico
        /// </summary>
        public async Task<List<TicketDTO>> GetTicketsByClienteAsync(Guid clienteId, int pageNumber = 1, int pageSize = 20)
        {
            var tickets = await _context.Tickets
                .Include(t => t.Rifa)
                .Where(t => t.ClienteId == clienteId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<TicketDTO>>(tickets);
        }
    }
}
