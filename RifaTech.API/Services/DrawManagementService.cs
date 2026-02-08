using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;
using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Services
{
    public class DrawManagementService : IDrawManagementService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DrawManagementService> _logger;
        private readonly IRifaService _rifaService;
        private readonly IClienteService _clienteService;

        public DrawManagementService(
            AppDbContext context,
            INotificationService notificationService,
            ILogger<DrawManagementService> logger,
            IRifaService rifaService,
            IClienteService clienteService)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _rifaService = rifaService;
            _clienteService = clienteService;
        }

        /// <summary>
        /// Executa o sorteio para uma rifa específica
        /// </summary>
        public async Task<DrawResultDTO> ExecuteDrawAsync(Guid rifaId)
        {
            // Buscar a rifa
            var rifa = await _context.Rifas
                .Include(r => r.Tickets.Where(t => t.EhValido))
                .ThenInclude(t => t.Cliente)
                .FirstOrDefaultAsync(r => r.Id == rifaId);

            if (rifa == null)
            {
                throw new KeyNotFoundException($"Rifa com ID {rifaId} não encontrada");
            }

            // Verificar se tem tickets válidos
            var validTickets = rifa.Tickets.Where(t => t.EhValido).ToList();
            if (!validTickets.Any())
            {
                throw new InvalidOperationException("Não há tickets válidos para realizar o sorteio");
            }

            // Gerar um número aleatório entre os tickets vendidos
            Random random = new Random();
            int winnerIndex = random.Next(validTickets.Count);
            var winningTicket = validTickets[winnerIndex];

            // Registrar o resultado do sorteio
            var draw = new Draw
            {
                RifaId = rifaId,
                DrawDateTime = DateTime.UtcNow,
                WinningNumber = winningTicket.Number.ToString()
            };

            _context.Draws.Add(draw);

            // Atualizar a rifa com o número vencedor
            rifa.WinningNumber = winningTicket.Number;

            await _context.SaveChangesAsync();

            // Preparar DTO do resultado
            var winnerCliente = winningTicket.Cliente;
            var drawResult = new DrawResultDTO
            {
                DrawId = draw.Id,
                RifaId = rifaId,
                RifaName = rifa.Name,
                DrawDateTime = draw.DrawDateTime,
                WinningNumber = winningTicket.Number,
                WinnerId = winnerCliente?.Id ?? Guid.Empty,
                WinnerName = winnerCliente?.Name ?? "Desconhecido",
                WinnerEmail = winnerCliente?.Email,
                WinnerPhone = winnerCliente?.PhoneNumber,
                PrizeValue = rifa.PriceValue
            };

            // Enviar notificações
            try
            {
                // Mapear DTOs para notificações
                var rifaDto = await _rifaService.GetRifaByIdAsync(rifaId);
                var drawDto = new DrawDTO
                {
                    Id = draw.Id,
                    RifaId = rifaId,
                    DrawDateTime = draw.DrawDateTime,
                    WinningNumber = draw.WinningNumber
                };

                // Lista de clientes participantes
                var participantIds = validTickets.Select(t => t.ClienteId).Distinct().ToList();
                var participants = new List<ClienteDTO>();

                foreach (var id in participantIds)
                {
                    var cliente = await _clienteService.GetClienteByIdAsync(id);
                    if (cliente != null)
                    {
                        participants.Add(cliente);
                    }
                }

                // Notificar todos os participantes
                await _notificationService.SendDrawResultAsync(rifaDto, drawDto, participants);

                // Notificar o vencedor separadamente
                if (winnerCliente != null)
                {
                    var winnerDto = await _clienteService.GetClienteByIdAsync(winnerCliente.Id);
                    if (winnerDto != null)
                    {
                        await _notificationService.SendWinnerNotificationAsync(
                            rifaDto,
                            drawDto,
                            winnerDto,
                            winningTicket.Number);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificações do resultado do sorteio");
                // Não interromper o processo se as notificações falharem
            }

            return drawResult;
        }

        /// <summary>
        /// Obtém o histórico de sorteios
        /// </summary>
        public async Task<List<DrawResultDTO>> GetDrawHistoryAsync(int count = 10)
        {
            var draws = await _context.Draws
                .Include(d => d.Rifa)
                .OrderByDescending(d => d.DrawDateTime)
                .Take(count)
                .ToListAsync();

            var results = new List<DrawResultDTO>();

            foreach (var draw in draws)
            {
                // Buscar o ticket vencedor
                var winningTicket = await _context.Tickets
                    .Include(t => t.Cliente)
                    .FirstOrDefaultAsync(t =>
                        t.RifaId == draw.RifaId &&
                        t.Number.ToString() == draw.WinningNumber &&
                        t.EhValido);

                var result = new DrawResultDTO
                {
                    DrawId = draw.Id,
                    RifaId = draw.RifaId,
                    RifaName = draw.Rifa.Name,
                    DrawDateTime = draw.DrawDateTime,
                    WinningNumber = int.Parse(draw.WinningNumber)
                };

                if (winningTicket != null && winningTicket.Cliente != null)
                {
                    result.WinnerId = winningTicket.Cliente.Id;
                    result.WinnerName = winningTicket.Cliente.Name;
                    result.WinnerEmail = winningTicket.Cliente.Email;
                    result.WinnerPhone = winningTicket.Cliente.PhoneNumber;
                }

                result.PrizeValue = draw.Rifa.PriceValue;

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Obtém uma prévia do sorteio para uma rifa específica
        /// </summary>
        public async Task<DrawPreviewDTO> GetDrawPreviewAsync(Guid rifaId)
        {
            var rifa = await _context.Rifas
                .Include(r => r.Tickets.Where(t => t.EhValido))
                .ThenInclude(t => t.Cliente)
                .FirstOrDefaultAsync(r => r.Id == rifaId);

            if (rifa == null)
            {
                throw new KeyNotFoundException($"Rifa com ID {rifaId} não encontrada");
            }

            var validTickets = rifa.Tickets.Where(t => t.EhValido).ToList();

            // Agrupar tickets por cliente
            var ticketsByCliente = validTickets
                .GroupBy(t => t.ClienteId)
                .Select(g => new DrawParticipantDTO
                {
                    ClienteId = g.Key,
                    ClienteName = g.First().Cliente?.Name ?? "Desconhecido",
                    TicketCount = g.Count(),
                    TicketNumbers = g.Select(t => t.Number).ToList(),
                    WinningChance = validTickets.Count > 0
                        ? (double)g.Count() / validTickets.Count * 100
                        : 0
                })
                .OrderByDescending(c => c.TicketCount)
                .ToList();

            return new DrawPreviewDTO
            {
                RifaId = rifaId,
                RifaName = rifa.Name,
                ScheduledDateTime = rifa.DrawDateTime,
                TotalValidTickets = validTickets.Count,
                TotalParticipants = ticketsByCliente.Count,
                PrizeValue = rifa.PriceValue,
                Participants = ticketsByCliente
            };
        }

        /// <summary>
        /// Agenda um sorteio para uma data específica
        /// </summary>
        public async Task<bool> ScheduleDrawAsync(Guid rifaId, DateTime drawDateTime)
        {
            var rifa = await _context.Rifas.FindAsync(rifaId);
            if (rifa == null)
            {
                throw new KeyNotFoundException($"Rifa com ID {rifaId} não encontrada");
            }

            // Verificar se a data é futura
            if (drawDateTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("A data do sorteio deve ser futura");
            }

            rifa.DrawDateTime = drawDateTime;
            await _context.SaveChangesAsync();

            // Poderia implementar aqui um agendamento de notificações para os participantes

            return true;
        }

        /// <summary>
        /// Cancela um sorteio agendado
        /// </summary>
        public async Task<bool> CancelDrawAsync(Guid rifaId)
        {
            var rifa = await _context.Rifas.FindAsync(rifaId);
            if (rifa == null)
            {
                throw new KeyNotFoundException($"Rifa com ID {rifaId} não encontrada");
            }

            // Verificar se o sorteio já foi realizado
            if (rifa.WinningNumber.HasValue)
            {
                throw new InvalidOperationException("Este sorteio já foi realizado e não pode ser cancelado");
            }

            // Procurar por um registro de Draw existente e remover
            var existingDraw = await _context.Draws
                .FirstOrDefaultAsync(d => d.RifaId == rifaId && !string.IsNullOrEmpty(d.WinningNumber));

            if (existingDraw != null)
            {
                _context.Draws.Remove(existingDraw);
            }

            // Agendar para uma data muito futura (efetivamente cancelando)
            rifa.DrawDateTime = DateTime.UtcNow.AddYears(10);
            rifa.EhDeleted = true; // Opcional: marcar como deletada

            await _context.SaveChangesAsync();

            return true;
        }
    }
}