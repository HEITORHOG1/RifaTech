using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.UI.Shared.Config
{
    /// <summary>
    /// Configurações centralizadas da aplicação
    /// </summary>
    public static class AppConfig
    {
        // URLs da API
        public static class Api
        {
            public const string BaseUrl = "https://localhost:7212";

            // Endpoints específicos
            public static class Endpoints
            {
                // Rifas
                public const string Rifas = "/api/rifas";
                public const string RifaById = "/api/rifas/{0}"; // Formato: /api/rifas/guid
                public const string RifasPaginated = "/api/rifas/paginated";
                public const string RifasDestaque = "/api/rifas/destaque";

                // Tickets
                public const string Tickets = "/api/tickets";
                public const string TicketsByRifa = "/api/rifa/{0}/tickets"; // Formato: /api/rifa/guid/tickets
                public const string TicketById = "/api/ticket/{0}"; // Formato: /api/ticket/guid
                public const string BuyTicket = "/api/rifa/{0}/buy-ticket"; // Formato: /api/rifa/guid/buy-ticket

                // Clientes
                public const string Clientes = "/api/clientes";
                public const string ClienteById = "/api/clientes/{0}"; // Formato: /api/clientes/guid
                public const string ClienteLookup = "/api/clientes/lookup"; // Busca por email, telefone ou CPF

                // Pagamentos
                public const string Payments = "/api/payments";
                public const string PaymentById = "/api/payments/{0}"; // Formato: /api/payments/guid
                public const string PaymentStatus = "/api/payments/status/{0}"; // Formato: /api/payments/status/guid
                public const string PaymentPix = "/api/payments/pix"; // Gerar pagamento PIX

                // Compra Rápida
                public const string CompraRapida = "/api/compra-rapida/{0}"; // Formato: /api/compra-rapida/guid

                // Autenticação
                public const string Auth = "/api/manage";
                public const string Login = "/api/manage/login";
                public const string Register = "/api/manage/register";
                public const string RefreshToken = "/api/manage/refresh-token";
                public const string UserInfo = "/api/manage/info";
                public const string Users = "/api/manage/users";

                // Admin
                public const string AdminStats = "/api/admin/dashboard/stats";
                public const string AdminSalesReport = "/api/admin/sales/report";
                public const string AdminTopSellingRifas = "/api/admin/rifas/top-selling";
                public const string AdminRecentSales = "/api/admin/tickets/recent-sales";
                public const string AdminUpcomingDraws = "/api/admin/draws/upcoming";
            }
        }

        // Configurações de autenticação e armazenamento local
        public static class LocalStorage
        {
            public const string AuthTokenKey = "authToken";
            public const string RefreshTokenKey = "refreshToken";
            public const string UserEmailKey = "email";
            public const string ThemeModeKey = "themeMode";
            public const string RecentSearchesKey = "recentSearches";
            public const string LastViewedRifasKey = "lastViewedRifas";
        }

        // Configurações UI
        public static class UI
        {
            public const int DefaultPageSize = 12;
            public const int DefaultCarouselInterval = 5000; // milissegundos
            public const string DefaultImagePath = "/images/default-rifa.jpg";
            public const string LogoPath = "/images/logo.png";

            // Cores (podem ser usadas para referência em CSS)
            public static class Colors
            {
                public const string Primary = "#1E88E5";
                public const string Secondary = "#FF4081";
                public const string Success = "#4CAF50";
                public const string Warning = "#FFC107";
                public const string Error = "#F44336";
                public const string Info = "#2196F3";
            }
        }

        // Configurações de integração e pagamentos
        public static class Payment
        {
            public const int PixExpirationMinutes = 30;
            public const int StatusCheckIntervalSeconds = 10;
            public const string PixIconPath = "/images/pix-logo.png";
        }

        // Tempo e formatação
        public static class Formatting
        {
            public const string DateFormat = "dd/MM/yyyy";
            public const string TimeFormat = "HH:mm";
            public const string DateTimeFormat = "dd/MM/yyyy HH:mm";
            public const string CurrencyFormat = "C2"; // R$ 0,00
            public const string CurrencyCulture = "pt-BR";
        }
    }
}
