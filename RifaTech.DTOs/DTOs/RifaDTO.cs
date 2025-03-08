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
        public float TicketPrice { get; set; }
        public DateTime DrawDateTime { get; set; }
        public int WinningNumber { get; set; }
        public int MinTickets { get; set; }
        public int MaxTickets { get; set; }
        public string? Base64Img { get; set; }

        // Novos campos
        public string? UserId { get; set; } // ID do usuário que criou a rifa
        public string? RifaLink { get; set; } // Link da rifa
        public string? UniqueId { get; set; } // Campo para o link único

        // Relacionamento com Tickets
        public List<TicketDTO> Tickets { get; set; } = new();

        // Relacionamento com Números Extras
        public List<ExtraNumberDTO> ExtraNumbers { get; set; } = new();

        public bool? EhDeleted { get; set; }
        public decimal PriceValue { get; set; }

        // Campos calculados para UI
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ProgressPercentage { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TimeRemaining { get; set; }

        // Informações do sorteio
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsDone => DrawDateTime < DateTime.UtcNow;

        // Estatísticas
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TicketsSold => Tickets?.Count ?? 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TicketsRemaining => MaxTickets - (Tickets?.Count ?? 0);
    }
}