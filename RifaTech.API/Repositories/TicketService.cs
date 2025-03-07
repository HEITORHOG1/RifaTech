using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public TicketService(AppDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<IEnumerable<TicketDTO>> GetTicketsByRifaAsync(Guid rifaId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.RifaId == rifaId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TicketDTO>>(tickets);
        }

        public async Task<TicketDTO> GetTicketByIdAsync(string id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            return ticket != null ? _mapper.Map<TicketDTO>(ticket) : null;
        }

        public async Task<IEnumerable<TicketDTO>> GetTicketsByUserAsync(Guid userId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.ClienteId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TicketDTO>>(tickets);
        }

        public async Task CancelTicketAsync(string id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TicketDTO> UpdateTicketAsync(string id, TicketDTO updatedTicketDto)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return null;
            }
            ticket.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(updatedTicketDto, ticket);
            await _context.SaveChangesAsync();

            return _mapper.Map<TicketDTO>(ticket);
        }

        public async Task<List<int>> PurchaseTicketAsync(string rifaId, TicketDTO ticketDto)
        {
            var rifa = await _context.Rifas.FindAsync(Guid.Parse(rifaId));
            if (rifa == null) throw new Exception("Rifa não encontrada.");

            // Gerar números aleatórios
            var numerosAleatorios = GerarNumerosAleatorios(rifa.Id, ticketDto.Quantidade);

            // Criar tickets
            var tickets = numerosAleatorios.Select(numero => new Ticket
            {
                RifaId = rifa.Id,
                ClienteId = ticketDto.ClienteId,
                Number = numero,
                EhValido = false
            }).ToList();

            // Adicionar tickets no banco
            _context.Tickets.AddRange(tickets);
            await _context.SaveChangesAsync();
            return numerosAleatorios;
        }

        private List<int> GerarNumerosAleatorios(Guid rifaId, int quantidade)
        {
            var rifa = _context.Rifas.Find(rifaId);
            var numerosExistentes = _context.Tickets.Where(t => t.RifaId == rifaId).Select(t => t.Number).ToList();
            var numerosAleatorios = new List<int>();
            var random = new Random();

            while (numerosAleatorios.Count < quantidade)
            {
                var numero = random.Next(1, rifa.MaxTickets);
                if (!numerosExistentes.Contains(numero))
                {
                    numerosAleatorios.Add(numero);
                }
            }
            return numerosAleatorios;
        }
    }
}