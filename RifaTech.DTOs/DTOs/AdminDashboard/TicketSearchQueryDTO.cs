namespace RifaTech.DTOs.DTOs.AdminDashboard
{
    /// <summary>
    /// Consulta para pesquisa de tickets
    /// </summary>
    public class TicketSearchQueryDTO
    {
        // Filtros primários
        public Guid? RifaId { get; set; }

        public Guid? ClienteId { get; set; }
        public int? TicketNumber { get; set; }

        // Filtros secundários
        public string RifaName { get; set; }

        public string ClienteName { get; set; }
        public string ClienteEmail { get; set; }
        public string ClientePhone { get; set; }
        public bool? ValidOnly { get; set; }

        // Filtros de data
        public DateTime? CreatedAfter { get; set; }

        public DateTime? CreatedBefore { get; set; }

        // Ordenação
        public string OrderBy { get; set; } = "createdAt";

        public string OrderDirection { get; set; } = "desc";

        // Paginação
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Resultado da pesquisa de tickets
    /// </summary>
    public class TicketSearchResultDTO
    {
        public List<TicketDTO> Tickets { get; set; } = new List<TicketDTO>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// Resumo de tickets para uma rifa
    /// </summary>
    public class TicketSummaryDTO
    {
        public Guid RifaId { get; set; }
        public string RifaName { get; set; }
        public int TotalTickets { get; set; }
        public int ValidTickets { get; set; }
        public int PendingTickets { get; set; }
        public double PercentageSold { get; set; }
        public List<TopBuyerDTO> TopBuyers { get; set; } = new List<TopBuyerDTO>();
        public List<TicketRangeDTO> NumberRanges { get; set; } = new List<TicketRangeDTO>();
        public List<DailySalesDTO> DailySales { get; set; } = new List<DailySalesDTO>();
    }

    /// <summary>
    /// Dados de um comprador frequente
    /// </summary>
    public class TopBuyerDTO
    {
        public Guid ClienteId { get; set; }
        public string ClienteName { get; set; }
        public string ClienteEmail { get; set; }
        public int TicketCount { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Faixa de números de tickets
    /// </summary>
    public class TicketRangeDTO
    {
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }
        public int SoldCount { get; set; }
        public int AvailableCount { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Vendas diárias
    /// </summary>
    public class DailySalesDTO
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int ValidCount { get; set; }
    }
}