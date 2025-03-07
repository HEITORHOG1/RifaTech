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

        Task<PaymentDTO> IniciarPagamentoPix(RifaDTO rifa, int quantidade, decimal valorTotal, ClienteDTO cliente);
    }
}