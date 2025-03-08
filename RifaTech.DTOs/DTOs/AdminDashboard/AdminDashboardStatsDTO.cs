using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.DTOs.DTOs.AdminDashboard
{
    /// <summary>
    /// Dados gerais do dashboard administrativo
    /// </summary>
    public class AdminDashboardStatsDTO
    {
        // Contagens totais
        public int TotalRifas { get; set; }
        public int ActiveRifas { get; set; }
        public int TotalTickets { get; set; }
        public int TotalClientes { get; set; }

        // Vendas por período
        public int TodaySales { get; set; }
        public int YesterdaySales { get; set; }
        public int WeekSales { get; set; }
        public int MonthSales { get; set; }

        // Receita por período
        public decimal TodayRevenue { get; set; }
        public decimal YesterdayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal MonthRevenue { get; set; }

        // Métricas adicionais
        public float ConversionRate { get; set; }
        public int UpcomingDraws { get; set; }

        // Metadados
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Relatório de vendas para um período específico
    /// </summary>
    public class SalesReportDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageDailySales { get; set; }
        public decimal AverageDailyRevenue { get; set; }
        public List<DailySalesData> DailySales { get; set; } = new List<DailySalesData>();
        public List<PaymentMethodData> PaymentMethods { get; set; } = new List<PaymentMethodData>();
    }

    /// <summary>
    /// Dados de vendas diárias
    /// </summary>
    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Dados de métodos de pagamento
    /// </summary>
    public class PaymentMethodData
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Dados de rifas mais vendidas
    /// </summary>
    public class TopSellingRifaDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal TicketPrice { get; set; }
        public DateTime DrawDateTime { get; set; }
        public int TicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PercentageSold { get; set; }
    }

    /// <summary>
    /// Dados de vendas recentes de tickets
    /// </summary>
    public class RecentTicketSaleDTO
    {
        public Guid TicketId { get; set; }
        public Guid RifaId { get; set; }
        public string RifaName { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteName { get; set; }
        public int TicketNumber { get; set; }
        public DateTime PurchaseTime { get; set; }
        public decimal TicketPrice { get; set; }
    }

    /// <summary>
    /// Dados de sorteios próximos
    /// </summary>
    public class UpcomingDrawDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DrawDateTime { get; set; }
        public string TimeRemaining { get; set; }
        public int TicketsSold { get; set; }
        public int MaxTickets { get; set; }
        public decimal PercentageSold { get; set; }
    }
}
