using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;
using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Services
{
    public class AdminStatsService : IAdminStatsService
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AdminStatsService> _logger;

        public AdminStatsService(
            AppDbContext context,
            ICacheService cacheService,
            ILogger<AdminStatsService> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AdminDashboardStatsDTO> GetDashboardStatsAsync()
        {
            return await _cacheService.GetOrCreateAsync("admin_dashboard_stats", async () =>
            {
                _logger.LogInformation("Cache miss for admin dashboard stats, calculating from database");

                var now = DateTime.UtcNow;
                var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                var yesterdayStart = todayStart.AddDays(-1);
                var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                // Total counts
                // Corrigido: Convertendo bool? para bool explicitamente
                var totalRifas = await _context.Rifas.CountAsync(r => r.EhDeleted == false);
                var totalTickets = await _context.Tickets.CountAsync(t => t.EhValido);
                var totalClientes = await _context.Clientes.CountAsync();

                // Active rifas (not yet drawn)
                // Corrigido: Convertendo bool? para bool explicitamente
                var activeRifas = await _context.Rifas.CountAsync(r => r.EhDeleted == false && r.DrawDateTime > now);

                // Sales counts
                var todaySales = await _context.Tickets
                    .Where(t => t.GeneratedTime >= todayStart && t.EhValido)
                    .CountAsync();

                var yesterdaySales = await _context.Tickets
                    .Where(t => t.GeneratedTime >= yesterdayStart && t.GeneratedTime < todayStart && t.EhValido)
                    .CountAsync();

                var weekSales = await _context.Tickets
                    .Where(t => t.GeneratedTime >= weekStart && t.EhValido)
                    .CountAsync();

                var monthSales = await _context.Tickets
                    .Where(t => t.GeneratedTime >= monthStart && t.EhValido)
                    .CountAsync();

                // Sales values
                var todayRevenue = await _context.Payments
                    .Where(p => p.CreatedAt >= todayStart && p.Status == PaymentStatus.Confirmed)
                    .SumAsync(p => (decimal)p.Amount);

                var yesterdayRevenue = await _context.Payments
                    .Where(p => p.CreatedAt >= yesterdayStart && p.CreatedAt < todayStart && p.Status == PaymentStatus.Confirmed)
                    .SumAsync(p => (decimal)p.Amount);

                var weekRevenue = await _context.Payments
                    .Where(p => p.CreatedAt >= weekStart && p.Status == PaymentStatus.Confirmed)
                    .SumAsync(p => (decimal)p.Amount);

                var monthRevenue = await _context.Payments
                    .Where(p => p.CreatedAt >= monthStart && p.Status == PaymentStatus.Confirmed)
                    .SumAsync(p => (decimal)p.Amount);

                // Conversion rate (percentage of successful payments)
                var todayAttempts = await _context.Payments
                    .Where(p => p.CreatedAt >= todayStart)
                    .CountAsync();

                var todaySuccessful = await _context.Payments
                    .Where(p => p.CreatedAt >= todayStart && p.Status == PaymentStatus.Confirmed)
                    .CountAsync();

                var conversionRate = todayAttempts > 0
                    ? (float)todaySuccessful / todayAttempts * 100
                    : 0;

                // Upcoming draws (next 7 days)
                // Corrigido: Convertendo bool? para bool explicitamente
                var upcomingDraws = await _context.Rifas
                    .Where(r => r.EhDeleted == false && r.DrawDateTime > now && r.DrawDateTime <= now.AddDays(7))
                    .CountAsync();

                return new AdminDashboardStatsDTO
                {
                    TotalRifas = totalRifas,
                    ActiveRifas = activeRifas,
                    TotalTickets = totalTickets,
                    TotalClientes = totalClientes,

                    TodaySales = todaySales,
                    YesterdaySales = yesterdaySales,
                    WeekSales = weekSales,
                    MonthSales = monthSales,

                    TodayRevenue = todayRevenue,
                    YesterdayRevenue = yesterdayRevenue,
                    WeekRevenue = weekRevenue,
                    MonthRevenue = monthRevenue,

                    ConversionRate = conversionRate,
                    UpcomingDraws = upcomingDraws,

                    LastUpdated = DateTime.UtcNow
                };
            }, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
        }

        public async Task<SalesReportDTO> GetSalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            // Format cache key with date range
            string cacheKey = $"sales_report_{start:yyyyMMdd}_{end:yyyyMMdd}";

            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation($"Cache miss for sales report ({start:yyyy-MM-dd} to {end:yyyy-MM-dd}), calculating from database");

                // Daily sales data
                var dailySales = await _context.Tickets
                    .Where(t => t.GeneratedTime >= start && t.GeneratedTime <= end && t.EhValido)
                    .GroupBy(t => new { Year = t.GeneratedTime.Year, Month = t.GeneratedTime.Month, Day = t.GeneratedTime.Day })
                    .Select(g => new DailySalesData
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                        Count = g.Count(),
                        // We need to join with Rifa to get the ticket price
                        Revenue = g.Sum(t => t.Rifa.TicketPrice)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                // Payment method breakdown
                var paymentMethods = await _context.Payments
                    .Where(p => p.CreatedAt >= start && p.CreatedAt <= end && p.Status == PaymentStatus.Confirmed)
                    .GroupBy(p => p.Method)
                    .Select(g => new PaymentMethodData
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        Total = g.Sum(p => (decimal)p.Amount)
                    })
                    .ToListAsync();

                // Total stats
                var totalSales = dailySales.Sum(x => x.Count);
                var totalRevenue = dailySales.Sum(x => x.Revenue);
                var averageDailySales = dailySales.Count > 0 ? totalSales / dailySales.Count : 0;
                var averageDailyRevenue = dailySales.Count > 0 ? totalRevenue / dailySales.Count : 0;

                return new SalesReportDTO
                {
                    StartDate = start,
                    EndDate = end,
                    TotalSales = totalSales,
                    TotalRevenue = totalRevenue,
                    AverageDailySales = averageDailySales,
                    AverageDailyRevenue = averageDailyRevenue,
                    DailySales = dailySales,
                    PaymentMethods = paymentMethods
                };
            }, TimeSpan.FromHours(1)); // Cache for 1 hour
        }

        public async Task<List<TopSellingRifaDTO>> GetTopSellingRifasAsync(int count = 5)
        {
            string cacheKey = $"top_selling_rifas_{count}";

            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation($"Cache miss for top selling rifas (count: {count}), calculating from database");

                // Corrigido: Convertendo bool? para bool explicitamente
                return await _context.Rifas
                    .Where(r => r.EhDeleted == false)
                    .Select(r => new TopSellingRifaDTO
                    {
                        Id = r.Id,
                        Name = r.Name,
                        TicketPrice = r.TicketPrice,
                        DrawDateTime = r.DrawDateTime,
                        TicketsSold = r.Tickets.Count(t => t.EhValido),
                        TotalRevenue = r.Tickets.Count(t => t.EhValido) * r.TicketPrice,
                        PercentageSold = r.MaxTickets > 0
                            ? (decimal)r.Tickets.Count(t => t.EhValido) / r.MaxTickets * 100
                            : 0
                    })
                    .OrderByDescending(r => r.TicketsSold)
                    .Take(count)
                    .ToListAsync();
            }, TimeSpan.FromMinutes(15)); // Cache for 15 minutes
        }

        public async Task<List<RecentTicketSaleDTO>> GetRecentTicketSalesAsync(int count = 10)
        {
            // This is real-time data, so we'll use a shorter cache time
            string cacheKey = $"recent_ticket_sales_{count}";

            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation($"Cache miss for recent ticket sales (count: {count}), calculating from database");

                return await _context.Tickets
                    .Where(t => t.EhValido)
                    .OrderByDescending(t => t.GeneratedTime)
                    .Take(count)
                    .Select(t => new RecentTicketSaleDTO
                    {
                        TicketId = t.Id,
                        RifaId = t.RifaId,
                        RifaName = t.Rifa.Name,
                        ClienteId = t.ClienteId,
                        ClienteName = t.Cliente.Name,
                        TicketNumber = t.Number,
                        PurchaseTime = t.GeneratedTime,
                        TicketPrice = t.Rifa.TicketPrice
                    })
                    .ToListAsync();
            }, TimeSpan.FromMinutes(1)); // Cache for just 1 minute
        }

        public async Task<List<UpcomingDrawDTO>> GetUpcomingDrawsAsync(int count = 5)
        {
            string cacheKey = $"upcoming_draws_{count}";

            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation($"Cache miss for upcoming draws (count: {count}), calculating from database");

                var now = DateTime.UtcNow;

                // Corrigido: Convertendo bool? para bool explicitamente
                return await _context.Rifas
                    .Where(r => r.EhDeleted == false && r.DrawDateTime > now)
                    .OrderBy(r => r.DrawDateTime)
                    .Take(count)
                    .Select(r => new UpcomingDrawDTO
                    {
                        Id = r.Id,
                        Name = r.Name,
                        DrawDateTime = r.DrawDateTime,
                        TimeRemaining = r.DrawDateTime.Subtract(now).ToString(@"dd\d\ hh\h\ mm\m"),
                        TicketsSold = r.Tickets.Count(t => t.EhValido),
                        MaxTickets = r.MaxTickets,
                        PercentageSold = r.MaxTickets > 0
                            ? (decimal)r.Tickets.Count(t => t.EhValido) / r.MaxTickets * 100
                            : 0
                    })
                    .ToListAsync();
            }, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
        }
    }
}