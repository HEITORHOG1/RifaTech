using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Services
{
    public interface ICompraRapidaService
    {
        Task<CompraRapidaResponseDTO> ProcessarCompraRapidaAsync(string rifaId, CompraRapidaDTO compraDto);
    }

    public class CompraRapidaService : ICompraRapidaService
    {
        private readonly IRifaService _rifaService;
        private readonly IClienteService _clienteService;
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CompraRapidaService> _logger;

        public CompraRapidaService(
            IRifaService rifaService,
            IClienteService clienteService,
            ITicketService ticketService,
            IPaymentService paymentService,
            INotificationService notificationService,
            ILogger<CompraRapidaService> logger)
        {
            _rifaService = rifaService;
            _clienteService = clienteService;
            _ticketService = ticketService;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<CompraRapidaResponseDTO> ProcessarCompraRapidaAsync(string rifaId, CompraRapidaDTO compraDto)
        {
            try
            {
                // Validate the rifa
                var rifaGuid = Guid.Parse(rifaId);
                var rifa = await _rifaService.GetRifaByIdAsync(rifaGuid);

                if (rifa == null)
                {
                    throw new KeyNotFoundException($"Rifa with ID {rifaId} not found");
                }

                if (rifa.DrawDateTime <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("This rifa has already ended");
                }

                // Validate ticket quantity
                int availableTickets = rifa.MaxTickets - (rifa.Tickets?.Count ?? 0);
                if (compraDto.Quantidade > availableTickets)
                {
                    throw new InvalidOperationException($"Not enough tickets available. Available: {availableTickets}");
                }

                // Find or create cliente
                ClienteDTO cliente;

                var existingCliente = await _clienteService.GetClienteByEmailOrPhoneNumberOrCPFAsync(
                    compraDto.Email,
                    compraDto.PhoneNumber,
                    compraDto.CPF
                );

                if (existingCliente == null)
                {
                    // Create new cliente
                    var newCliente = new ClienteDTO
                    {
                        Name = compraDto.Name,
                        Email = compraDto.Email,
                        PhoneNumber = compraDto.PhoneNumber,
                        CPF = compraDto.CPF
                    };

                    cliente = await _clienteService.CreateClienteAsync(newCliente);
                    _logger.LogInformation($"New client created: {cliente.Id}");
                }
                else
                {
                    cliente = existingCliente;
                    _logger.LogInformation($"Existing client found: {cliente.Id}");

                    // Update cliente information if needed
                    bool updateCliente = false;

                    if (!string.IsNullOrEmpty(compraDto.Name) && compraDto.Name != cliente.Name)
                    {
                        cliente.Name = compraDto.Name;
                        updateCliente = true;
                    }

                    if (!string.IsNullOrEmpty(compraDto.CPF) && compraDto.CPF != cliente.CPF)
                    {
                        cliente.CPF = compraDto.CPF;
                        updateCliente = true;
                    }

                    if (updateCliente)
                    {
                        await _clienteService.UpdateClienteAsync(cliente.Id, cliente);
                        _logger.LogInformation($"Client information updated: {cliente.Id}");
                    }
                }

                // Create ticket
                var ticketDto = new TicketDTO
                {
                    RifaId = rifaGuid,
                    ClienteId = cliente.Id,
                    Quantidade = compraDto.Quantidade
                };

                var generatedNumbers = await _ticketService.PurchaseTicketAsync(rifaId, ticketDto);
                _logger.LogInformation($"Generated {generatedNumbers.Count} tickets for rifa {rifaId}");

                // Calculate total value
                decimal totalValue = (decimal)rifa.TicketPrice * compraDto.Quantidade;

                // Generate PIX payment
                var payment = await _paymentService.IniciarPagamentoPix(rifa.Id, compraDto.Quantidade, totalValue, cliente.Id);
                _logger.LogInformation($"Generated PIX payment: {payment.Id}");

                // Prepare response
                var response = new CompraRapidaResponseDTO
                {
                    Success = true,
                    Message = "Purchase processed successfully",
                    Cliente = cliente,
                    Payment = payment,
                    NumerosGerados = generatedNumbers,
                    ValorTotal = totalValue,
                    RifaId = rifaGuid,
                    RifaNome = rifa.Name
                };

                // Send purchase confirmation notification
                try
                {
                    await _notificationService.SendPurchaseConfirmationAsync(response);
                    _logger.LogInformation($"Purchase confirmation notification sent to {cliente.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending purchase confirmation notification");
                    // Continue processing, do not throw
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing quick purchase for rifa {rifaId}");
                throw;
            }
        }
    }
}