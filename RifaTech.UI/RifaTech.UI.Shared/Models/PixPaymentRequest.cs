using RifaTech.DTOs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.UI.Shared.Models
{
    // RifaTech.UI.Shared/Models/PixPaymentRequest.cs

    public class PixPaymentRequest
    {
        public Guid RifaId { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
        public Guid ClienteId { get; set; }
    }
}
