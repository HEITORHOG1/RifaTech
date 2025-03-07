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

        public PaymentService(AppDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
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

        public async Task<PaymentDTO> IniciarPagamentoPix(RifaDTO rifa, int quantidade, decimal valorTotal, ClienteDTO cliente)
        {
            // Configurar o token de acesso do Mercado Pago
            MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];

            // Criar uma instância de PaymentClient
            var paymentClient = new PaymentClient();

            // Criar a requisição de pagamento
            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = valorTotal,
                Description = rifa.Name,
                PaymentMethodId = "pix",
                Payer = new PaymentPayerRequest
                {
                    Email = cliente.Email,  // Substituir pelo e-mail do cliente
                    FirstName = cliente.Name,
                    Phone = new PaymentPhoneRequest
                    {
                        AreaCode = cliente.PhoneNumber.Substring(0, 2),  // Substituir pelo DDD do cliente
                        Number = cliente.PhoneNumber.Substring(2)  // Substituir pelo número do cliente
                    },
                }
                // Outras configurações do pagamento, se necessário
            };

            // Enviar a requisição e obter a resposta
            MercadoPago.Resource.Payment.Payment paymentResponse;

            try
            {
                paymentResponse = await paymentClient.CreateAsync(paymentRequest);
            }
            catch (Exception ex)
            {
                // Tratar erros de comunicação com a API do Mercado Pago
                throw new Exception("Erro ao iniciar pagamento PIX", ex);
            }
            // Converter a resposta para a entidade Payment da aplicação
            var payment = new PaymentDTO
            {
                ClienteId = cliente.Id,  // Converter o ID do cliente
                TicketId = Guid.NewGuid(),  // Supondo que você gere um TicketId aqui
                Amount = (float)valorTotal,
                Method = "PIX",
                IsConfirmed = false,  // Inicialmente, o pagamento não está confirmado
                                      // Outros campos como QrCodeBase64, se necessário
                                      // Por exemplo:
                QrCodeBase64 = paymentResponse.PointOfInteraction.TransactionData.QrCodeBase64,
                QrCode = paymentResponse.PointOfInteraction.TransactionData.QrCode,
                PaymentId = paymentResponse.Id,
            };
            // Retornar a resposta do pagamento
            return payment;
        }
    }
}