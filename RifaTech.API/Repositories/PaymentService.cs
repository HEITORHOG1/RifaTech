using AutoMapper;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
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

        public PaymentService(
            AppDbContext context,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
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
            // Implemente a lógica para verificar o status do pagamento
            // Pode ser necessário consultar uma API externa ou verificar o banco de dados

            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                throw new Exception("Pagamento não encontrado.");
            }

            // Verifique o status do pagamento e atualize conforme necessário

            return new PaymentDTO
            {
                // Preencha com as informações atualizadas do pagamento
                Id = paymentId,
                Amount = payment.Amount,
                Method = payment.Method,
                IsConfirmed = payment.IsConfirmed,
                // Outros campos, se necessário
            };
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

                // Configurar o token de acesso do Mercado Pago
                string accessToken = _configuration["MercadoPago:AccessToken"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Token de acesso do Mercado Pago não configurado");
                }

                MercadoPagoConfig.AccessToken = accessToken;

                if (quantidade <= 0)
                {
                    throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
                }

                if (valorTotal <= 0)
                {
                    throw new ArgumentException("Valor total deve ser maior que zero", nameof(valorTotal));
                }

                // Criar a requisição de pagamento
                var paymentClient = new PaymentClient();

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
                        // Remova a propriedade Identification temporariamente
                        Phone = new PaymentPayerPhoneRequest
                        {
                            AreaCode = areaCode,
                            Number = number
                        }
                    }
                };

                // Enviar a requisição e obter a resposta
                MercadoPago.Resource.Payment.Payment paymentResponse;

                try
                {
                    paymentResponse = await paymentClient.CreateAsync(paymentRequest);
                    _logger.LogInformation($"Pagamento PIX criado: ID {paymentResponse.Id}");
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
                    Amount = (float)valorTotal,
                    IsConfirmed = false,
                    QrCodeBase64 = paymentResponse.PointOfInteraction?.TransactionData?.QrCodeBase64 ?? "",
                    QrCode = paymentResponse.PointOfInteraction?.TransactionData?.QrCode ?? "",
                    Status = PaymentStatus.Pending,
                    ExpirationTime = expiracao,
                    PaymentId = paymentResponse.Id
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
                _logger.LogError(ex, $"Erro ao iniciar pagamento PIX para rifa");
                throw;
            }
        }

        // Método auxiliar para atualizar o status do ticket
        private async Task UpdateTicketStatusAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.EhValido = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Ticket {ticketId} atualizado para válido após confirmação de pagamento");
            }
        }
    }
}