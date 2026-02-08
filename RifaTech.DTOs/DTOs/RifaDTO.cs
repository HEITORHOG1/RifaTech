using System.Text.Json.Serialization;

namespace RifaTech.DTOs.DTOs
{
    public class RifaDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Preço do ticket. Usar decimal para precisão monetária.
        /// </summary>
        public decimal TicketPrice { get; set; }

        public DateTime DrawDateTime { get; set; }
        public int WinningNumber { get; set; }
        public int MinTickets { get; set; }
        public int MaxTickets { get; set; }
        public string? Base64Img { get; set; }

        public string? UserId { get; set; }
        public string? RifaLink { get; set; }
        public string? UniqueId { get; set; }

        public List<TicketDTO> Tickets { get; set; } = new();
        public List<ExtraNumberDTO> ExtraNumbers { get; set; } = new();

        public bool? EhDeleted { get; set; }
        public decimal PriceValue { get; set; }

        // Campos calculados (preenchidos pelo serviço, não computados no DTO)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ProgressPercentage { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TimeRemaining { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsDone { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TicketsSold { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TicketsRemaining { get; set; }
    }
}