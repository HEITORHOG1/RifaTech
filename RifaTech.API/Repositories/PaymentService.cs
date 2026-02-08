using AutoMapper;
using MercadoPago.Client.Payment;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.API.Services;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Repositories
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly IMercadoPagoService _mercadoPagoService;

        public PaymentService(
            AppDbContext context,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<PaymentService> logger,
            IMercadoPagoService mercadoPagoService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _mercadoPagoService = mercadoPagoService;
        }

        public async Task<PaymentDTO> ProcessPaymentAsync(PaymentDTO paymentDto)
        {
            var payment = _mapper.Map<Payment>(paymentDto);

            // Aqui você pode adicionar lógica adicional para processar o pagamento
            // Por exemplo, verificar se o pagamento foi confirmado

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO> GetPaymentByIdAsync(Guid id)
        {
            var payment = await _context.Payments.FindAsync(id);
            return payment != null ? _mapper.Map<PaymentDTO>(payment) : null;
        }

        public async Task<IEnumerable<PaymentDTO>> GetPaymentsByUserAsync(Guid userId)
        {
            var payments = await _context.Payments
                .Where(p => p.ClienteId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        public async Task<IEnumerable<PaymentDTO>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments.ToListAsync();
            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        public async Task<PaymentDTO> CheckPaymentStatusAsync(Guid paymentId)
        {
            // Buscar o pagamento no banco de dados
            var payment = await _context.Payments
                .Include(p => p.Ticket)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new KeyNotFoundException($"Pagamento com ID {paymentId} não encontrado");
            }

            // Se o pagamento já foi confirmado, retorne imediatamente
            if (payment.Status == PaymentStatus.Confirmed)
            {
                return _mapper.Map<PaymentDTO>(payment);
            }

            // Se o pagamento já expirou, retorne imediatamente
            if (payment.ExpirationTime < DateTime.UtcNow && payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Expired;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Pagamento {payment.Id} foi marcado como expirado");
                return _mapper.Map<PaymentDTO>(payment);
            }

            // Se tivermos um ID de pagamento externo (Mercado Pago), verificar o status
            if (payment.PaymentId.HasValue)
            {
                try
                {
                    // Obter status do pagamento no Mercado Pago
                    var mpPayment = await _mercadoPagoService.GetPaymentStatusAsync(payment.PaymentId.Value);

                    _logger.LogInformation($"Status do pagamento {payment.Id} no Mercado Pago: {mpPayment.Status}");

                    // Mapear status do Mercado Pago para o status interno
                    var newStatus = MercadoPagoService.MapPaymentStatus(mpPayment.Status);

                    // Se houve mudança de status, atualizar no banco de dados
                    if (payment.Status != newStatus)
                    {
                        payment.Status = newStatus;

                        // Se for confirmado, atualizar a flag IsConfirmed
                        if (newStatus == PaymentStatus.Confirmed)
                        {
                            payment.IsConfirmed = true;

                            // Atualizar tickets associados
                            await UpdateTicketsForPaymentAsync(payment);
                        }

                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Pagamento {payment.Id} atualizado para status {newStatus}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao verificar status do pagamento {payment.Id} no Mercado Pago");
                    // Não propagar a exceção, apenas continuar
                }
            }

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO> IniciarPagamentoPix(Guid rifaId, int quantidade, decimal valorTotal, Guid clienteId)
        {
            try
            {
                // Buscar informações da rifa e do cliente
                var rifa = await _context.Rifas.FindAsync(rifaId);
                if (rifa == null)
                {
                    throw new ArgumentException($"Rifa com ID {rifaId} não encontrada", nameof(rifaId));
                }

                var cliente = await _context.Clientes.FindAsync(clienteId);
                if (cliente == null)
                {
                    throw new ArgumentException($"Cliente com ID {clienteId} não encontrado", nameof(clienteId));
                }

                if (quantidade <= 0)
                {
                    throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
                }

                if (valorTotal <= 0)
                {
                    throw new ArgumentException("Valor total deve ser maior que zero", nameof(valorTotal));
                }

                // Preparar telefone do cliente
                string phoneNumber = cliente.PhoneNumber?.Replace("+", "").Replace("-", "").Replace(" ", "").Trim() ?? "";
                string areaCode = "11"; // Padrão
                string number = phoneNumber;

                // Tentar extrair DDD
                if (phoneNumber.Length >= 2)
                {
                    areaCode = phoneNumber.Substring(0, 2);
                    number = phoneNumber.Substring(2);
                }

                // Criar descrição do pagamento
                string descricao = $"Compra de {quantidade} número(s) na rifa: {rifa.Name}";

                // Configurar expiração (30 minutos)
                DateTime expiracao = DateTime.UtcNow.AddMinutes(30);

                // Criar a requisição de pagamento
                var paymentRequest = new PaymentCreateRequest
                {
                    TransactionAmount = (decimal)valorTotal,
                    Description = descricao,
                    PaymentMethodId = "pix",
                    DateOfExpiration = expiracao,
                    Payer = new PaymentPayerRequest
                    {
                        Email = cliente.Email,
                        FirstName = cliente.Name?.Split(' ').FirstOrDefault() ?? "Cliente",
                        LastName = cliente.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                        Phone = new PaymentPayerPhoneRequest
                        {
                            AreaCode = areaCode,
                            Number = number
                        }
                    }
                };

                // Criar pagamento usando o serviço MercadoPago
                MercadoPago.Resource.Payment.Payment mercadoPagoPayment;
                try
                {
                    mercadoPagoPayment = await _mercadoPagoService.CreatePaymentAsync(paymentRequest);
                    _logger.LogInformation($"Pagamento PIX criado: ID {mercadoPagoPayment.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao iniciar pagamento PIX com Mercado Pago");
                    throw new Exception("Erro ao iniciar pagamento PIX. Por favor, tente novamente.", ex);
                }

                // Criar a entidade de pagamento
                var payment = new Payment
                {
                    ClienteId = cliente.Id,
                    Method = "PIX",
                    Amount = valorTotal,
                    IsConfirmed = false,
                    QrCodeBase64 = mercadoPagoPayment.PointOfInteraction?.TransactionData?.QrCodeBase64 ?? "",
                    QrCode = mercadoPagoPayment.PointOfInteraction?.TransactionData?.QrCode ?? "",
                    Status = PaymentStatus.Pending,
                    ExpirationTime = expiracao,
                    PaymentId = mercadoPagoPayment.Id
                };

                // Salvar o pagamento
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Salvo pagamento {payment.Id} no banco de dados");

                // Retornar o DTO
                return _mapper.Map<PaymentDTO>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao iniciar pagamento PIX para rifa {rifaId}");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDTO>> GetPaymentsByExternalIdAsync(long externalId)
        {
            var payments = await _context.Payments
                .Where(p => p.PaymentId == externalId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        private async Task UpdateTicketsForPaymentAsync(Payment payment)
        {
            try
            {
                // Se o pagamento tem um ticket associado, atualizar seu status
                if (payment.Ticket != null)
                {
                    payment.Ticket.EhValido = true;
                    _logger.LogInformation($"Ticket {payment.Ticket.Id} atualizado para válido após confirmação de pagamento");
                }

                // Também buscar todos os tickets que tenham este PaymentId
                var tickets = await _context.Tickets
                    .Where(t => t.PaymentId == payment.Id)
                    .ToListAsync();

                foreach (var ticket in tickets)
                {
                    ticket.EhValido = true;
                    _logger.LogInformation($"Ticket {ticket.Id} atualizado para válido após confirmação de pagamento");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar tickets para pagamento {payment.Id}");
                // Continue, não interrompa o processamento se falhar a atualização dos tickets
            }
        }
    }
}