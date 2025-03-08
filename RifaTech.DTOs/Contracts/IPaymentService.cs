using RifaTech.DTOs.DTOs;

namespace RifaTech.DTOs.Contracts
{
    public interface IPaymentService
    {
        Task<PaymentDTO> ProcessPaymentAsync(PaymentDTO payment);

        Task<PaymentDTO> GetPaymentByIdAsync(Guid id);

        Task<IEnumerable<PaymentDTO>> GetPaymentsByUserAsync(Guid userId);

        Task<IEnumerable<PaymentDTO>> GetAllPaymentsAsync();

        Task<PaymentDTO> CheckPaymentStatusAsync(Guid paymentId);

        Task<PaymentDTO> IniciarPagamentoPix(Guid rifaId, int quantidade, decimal valorTotal, Guid clienteId);

        // Método para buscar pagamentos por ID externo (Mercado Pago)
        Task<IEnumerable<PaymentDTO>> GetPaymentsByExternalIdAsync(long externalId);
    }
}