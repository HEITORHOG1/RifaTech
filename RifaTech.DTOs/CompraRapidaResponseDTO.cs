using RifaTech.DTOs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.DTOs
{
    public class CompraRapidaResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ClienteDTO Cliente { get; set; }
        public PaymentDTO Payment { get; set; }
        public List<int> NumerosGerados { get; set; }
        public decimal ValorTotal { get; set; }
        public Guid RifaId { get; set; }
        public string RifaNome { get; set; }

        // Informações adicionais para o frontend
        public string QrCodePix => Payment?.QrCodeBase64;
        public string CodigoPix => Payment?.QrCode;
        public DateTime? ExpiracaoPix => Payment?.ExpirationTime;
        public string StatusPagamento => Payment?.Status switch
        {
            0 => "Pendente",
            1 => "Confirmado",
            2 => "Expirado",
            _ => "Desconhecido"
        };

        // Link para verificação de status
        public string StatusVerificationEndpoint => $"/payments/status/{Payment?.Id}";
    }
}
