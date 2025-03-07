using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Exceptions
{
    public class PixPaymentRequest
    {
        public RifaDTO RifaId { get; set; }  // Assuming RifaDTO is the correct type for the incoming RifaId object
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
        public ClienteDTO ClienteId { get; set; }  // Assuming ClienteDTO is the correct type for the incoming ClienteId object
    }
}