using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.DTOs.DTOs.AdminDashboard
{
    /// <summary>
    /// Resultado de um sorteio
    /// </summary>
    public class DrawResultDTO
    {
        public Guid DrawId { get; set; }
        public Guid RifaId { get; set; }
        public string RifaName { get; set; }
        public DateTime DrawDateTime { get; set; }
        public int WinningNumber { get; set; }
        public Guid WinnerId { get; set; }
        public string WinnerName { get; set; }
        public string WinnerEmail { get; set; }
        public string WinnerPhone { get; set; }
        public decimal PrizeValue { get; set; }
    }

    /// <summary>
    /// Prévia de um sorteio (dados antes da realização)
    /// </summary>
    public class DrawPreviewDTO
    {
        public Guid RifaId { get; set; }
        public string RifaName { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int TotalValidTickets { get; set; }
        public int TotalParticipants { get; set; }
        public decimal PrizeValue { get; set; }
        public List<DrawParticipantDTO> Participants { get; set; } = new List<DrawParticipantDTO>();
    }

    /// <summary>
    /// Dados de um participante do sorteio
    /// </summary>
    public class DrawParticipantDTO
    {
        public Guid ClienteId { get; set; }
        public string ClienteName { get; set; }
        public int TicketCount { get; set; }
        public List<int> TicketNumbers { get; set; } = new List<int>();
        public double WinningChance { get; set; }
    }

    /// <summary>
    /// Requisição para agendar um sorteio
    /// </summary>
    public class ScheduleDrawRequest
    {
        public Guid RifaId { get; set; }
        public DateTime DrawDateTime { get; set; }
        public bool NotifyParticipants { get; set; } = true;
    }
}
