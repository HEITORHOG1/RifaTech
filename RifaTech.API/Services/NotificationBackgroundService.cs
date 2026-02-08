using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Services
{
    /// <summary>
    /// Background service to handle scheduled notifications like draw reminders
    /// </summary>
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendDrawRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending scheduled notifications");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Notification Background Service is stopping.");
        }

        private async Task SendDrawRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var rifaService = scope.ServiceProvider.GetRequiredService<IRifaService>();
            var clienteService = scope.ServiceProvider.GetRequiredService<IClienteService>();
            var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();

            // Get upcoming draws in the next 24 hours
            var now = DateTime.UtcNow;
            var nextDay = now.AddHours(24);

            var upcomingRifas = await dbContext.Rifas
                .Where(r => r.EhDeleted == false &&
                            r.DrawDateTime > now &&
                            r.DrawDateTime <= nextDay)
                .ToListAsync();

            foreach (var rifa in upcomingRifas)
            {
                _logger.LogInformation($"Processing draw reminders for rifa: {rifa.Id} - {rifa.Name}");

                // Get all tickets for this rifa
                var tickets = await dbContext.Tickets
                    .Include(t => t.Cliente)
                    .Where(t => t.RifaId == rifa.Id && t.EhValido)
                    .ToListAsync();

                // Group tickets by cliente to avoid sending multiple notifications
                var clienteGroups = tickets
                    .GroupBy(t => t.ClienteId)
                    .ToList();

                foreach (var group in clienteGroups)
                {
                    var firstTicket = group.First();
                    if (firstTicket.Cliente == null || string.IsNullOrEmpty(firstTicket.Cliente.Email))
                    {
                        continue;
                    }

                    try
                    {
                        // Map to DTOs
                        var rifaDto = mapper.Map<RifaDTO>(rifa);
                        var clienteDto = mapper.Map<ClienteDTO>(firstTicket.Cliente);
                        var ticketNumbers = group.Select(t => t.Number).ToList();

                        // Send reminder
                        await notificationService.SendDrawReminderAsync(rifaDto, clienteDto, ticketNumbers);
                        _logger.LogInformation($"Sent draw reminder to {clienteDto.Email} for rifa {rifa.Name}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending draw reminder for cliente {firstTicket.Cliente.Id}");
                    }
                }
            }
        }
    }
}