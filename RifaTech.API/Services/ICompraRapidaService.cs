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
            _logger = logger;
        }

        public async Task<CompraRapidaResponseDTO> ProcessarCompraRapidaAsync(string rifaId, CompraRapidaDTO compraDto)
        {
            try
            {
                // Validar a rifa
                var rifaGuid = Guid.Parse(rifaId);
                var rifa = await _rifaService.GetRifaByIdAsync(rifaGuid);

                if (rifa == null)
                {
                    throw new KeyNotFoundException($"Rifa com ID {rifaId} não encontrada");
                }

                if (rifa.DrawDateTime <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Esta rifa já foi encerrada");
                }

                // Validar a quantidade de tickets
                int ticketsDisponiveis = rifa.MaxTickets - (rifa.Tickets?.Count ?? 0);
                if (compraDto.Quantidade > ticketsDisponiveis)
                {
                    throw new InvalidOperationException($"Não há tickets suficientes disponíveis. Disponíveis: {ticketsDisponiveis}");
                }

                // Buscar ou criar cliente
                ClienteDTO cliente;

                var clienteExistente = await _clienteService.GetClienteByEmailOrPhoneNumberOrCPFAsync(
                    compraDto.Email,
                    compraDto.PhoneNumber,
                    compraDto.CPF
                );

                if (clienteExistente == null)
                {
                    // Criar novo cliente
                    var novoCliente = new ClienteDTO
                    {
                        Name = compraDto.Name,
                        Email = compraDto.Email,
                        PhoneNumber = compraDto.PhoneNumber,
                        CPF = compraDto.CPF
                    };

                    cliente = await _clienteService.CreateClienteAsync(novoCliente);
                    _logger.LogInformation($"Novo cliente criado: {cliente.Id}");
                }
                else
                {
                    cliente = clienteExistente;
                    _logger.LogInformation($"Cliente existente encontrado: {cliente.Id}");

                    // Atualizar informações do cliente se necessário
                    bool atualizarCliente = false;

                    if (!string.IsNullOrEmpty(compraDto.Name) && compraDto.Name != cliente.Name)
                    {
                        cliente.Name = compraDto.Name;
                        atualizarCliente = true;
                    }

                    if (!string.IsNullOrEmpty(compraDto.CPF) && compraDto.CPF != cliente.CPF)
                    {
                        cliente.CPF = compraDto.CPF;
                        atualizarCliente = true;
                    }

                    if (atualizarCliente)
                    {
                        await _clienteService.UpdateClienteAsync(cliente.Id, cliente);
                        _logger.LogInformation($"Informações do cliente atualizadas: {cliente.Id}");
                    }
                }

                // Criar ticket
                var ticketDto = new TicketDTO
                {
                    RifaId = rifaGuid,
                    ClienteId = cliente.Id,
                    Quantidade = compraDto.Quantidade
                };

                var numerosGerados = await _ticketService.PurchaseTicketAsync(rifaId, ticketDto);
                _logger.LogInformation($"Gerados {numerosGerados.Count} tickets para a rifa {rifaId}");

                // Calcular valor total
                decimal valorTotal = (decimal)rifa.TicketPrice * compraDto.Quantidade;

                // Gerar pagamento PIX
                var payment = await _paymentService.IniciarPagamentoPix(rifa.Id, compraDto.Quantidade, valorTotal, cliente.Id);
                _logger.LogInformation($"Pagamento PIX gerado: {payment.Id}");

                // Montar resposta
                return new CompraRapidaResponseDTO
                {
                    Success = true,
                    Message = "Compra processada com sucesso",
                    Cliente = cliente,
                    Payment = payment,
                    NumerosGerados = numerosGerados,
                    ValorTotal = valorTotal,
                    RifaId = rifaGuid,
                    RifaNome = rifa.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar compra rápida para rifa {rifaId}");
                throw;
            }
        }
    }
}
