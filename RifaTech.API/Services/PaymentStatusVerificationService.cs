using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Services
{
    /// <summary>
    /// Serviço em background para verificar o status de pagamentos pendentes
    /// </summary>
    public class PaymentStatusVerificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentStatusVerificationService> _logger;
        private readonly AutoMapper.IMapper _mapper;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public PaymentStatusVerificationService(
            IServiceProvider serviceProvider,
            AutoMapper.IMapper mapper,
            ILogger<PaymentStatusVerificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _mapper = mapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Status Verification Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await VerifyPendingPayments();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while verifying payment statuses");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Payment Status Verification Service is stopping.");
        }

        private async Task VerifyPendingPayments()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            // Obter pagamentos pendentes com expiração próxima (últimos 15 minutos até 2 minutos no futuro)
            var now = DateTime.UtcNow;
            var pendingPayments = await dbContext.Payments
                .Where(p => p.Status == API.Entities.PaymentStatus.Pending)
                .Where(p => p.ExpirationTime > now.AddMinutes(-15) && p.ExpirationTime < now.AddMinutes(2))
                .ToListAsync();

            _logger.LogInformation($"Verificando {pendingPayments.Count} pagamentos pendentes");

            foreach (var payment in pendingPayments)
            {
                try
                {
                    // Verificar status do pagamento
                    if (payment.PaymentId.HasValue)
                    {
                        var checkedPayment = await paymentService.CheckPaymentStatusAsync(payment.Id);
                        _logger.LogInformation($"Payment ID {payment.Id}: Status = {checkedPayment.Status}");

                        // Atualizar o status do ticket se o pagamento foi confirmado
                        if (checkedPayment.Status == 1) // Confirmado
                        {
                            // Obter o ticket associado
                            var ticket = await dbContext.Tickets
                                .Include(t => t.Cliente)
                                .Include(t => t.Rifa)
                                .FirstOrDefaultAsync(t => t.Id == payment.TicketId);

                            if (ticket != null)
                            {
                                ticket.EhValido = true;
                                await dbContext.SaveChangesAsync();
                                _logger.LogInformation($"Ticket ID {ticket.Id} marcado como válido após confirmação de pagamento");

                                // Enviar notificação de pagamento confirmado
                                try
                                {
                                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                                    var rifaService = scope.ServiceProvider.GetRequiredService<IRifaService>();
                                    var clienteService = scope.ServiceProvider.GetRequiredService<IClienteService>();

                                    // Obter todas as informações necessárias
                                    var clienteDto = await clienteService.GetClienteByIdAsync(ticket.ClienteId);
                                    var rifaDto = await rifaService.GetRifaByIdAsync(ticket.RifaId);

                                    // Obter todos os números comprados pelo cliente nesta rifa
                                    var ticketNumbers = await dbContext.Tickets
                                        .Where(t => t.RifaId == ticket.RifaId && t.ClienteId == ticket.ClienteId)
                                        .Select(t => t.Number)
                                        .ToListAsync();

                                    // Enviar e-mail de confirmação
                                    await notificationService.SendPaymentConfirmationAsync(
                                        _mapper.Map<PaymentDTO>(payment),
                                        clienteDto,
                                        rifaDto,
                                        ticketNumbers);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Erro ao enviar notificação de pagamento confirmado para ticket {ticket.Id}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao verificar status do pagamento {payment.Id}");
                }
            }
        }
    }
}