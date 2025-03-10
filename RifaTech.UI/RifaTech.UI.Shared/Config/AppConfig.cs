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
                public const string Rifas = "/rifas";
                public const string RifaById = "/rifas/{0}"; // Formato: /rifas/guid
                public const string RifasPaginated = "/rifas/paginated";
                public const string RifasDestaque = "/rifas/destaque";
                public const string RifaMetrics = "/rifas/metrics";
                public const string RifaMarkAsDeleted = "/rifas/{0}/mark-as-deleted"; // Formato: /rifas/guid/mark-as-deleted

                // Tickets
                public const string Tickets = "/tickets";
                public const string TicketsByRifa = "/rifa/{0}/tickets"; // Formato: /rifa/guid/tickets
                public const string TicketById = "/ticket/{0}"; // Formato: /ticket/guid
                public const string BuyTicket = "/rifa/{0}/buy-ticket"; // Formato: /rifa/guid/buy-ticket
                public const string MyTickets = "/tickets/meus";
                public const string CancelTicket = "/ticket/{0}"; // Formato para DELETE: /ticket/guid

                // Clientes
                public const string Clientes = "/clientes";
                public const string ClienteById = "/clientes/{0}"; // Formato: /clientes/guid
                public const string ClienteLookup = "/clientes/lookup"; // Busca por email, telefone ou CPF
                public const string ClienteByEmailPhoneCpf = "/clientes/{0}/{1}/{2}"; // Formato: /clientes/email/phone/cpf

                // Pagamentos
                public const string Payments = "/payments";
                public const string PaymentById = "/payments/{0}"; // Formato: /payments/guid
                public const string PaymentStatus = "/payments/status/{0}"; // Formato: /payments/status/guid
                public const string PaymentPix = "/payments/pix"; // Gerar pagamento PIX

                // Compra Rápida
                public const string CompraRapida = "/compra-rapida/{0}"; // Formato: /compra-rapida/guid

                // Autenticação
                public const string Auth = "/manage";
                public const string Login = "/manage/login";
                public const string Register = "/manage/register";
                public const string RefreshToken = "/manage/refresh-token";
                public const string UserInfo = "/manage/info";
                public const string Users = "/manage/users";

                // Admin
                public const string AdminStats = "/admin/dashboard/stats";
                public const string AdminSalesReport = "/admin/sales/report";
                public const string AdminTopSellingRifas = "/admin/rifas/top-selling";
                public const string AdminRecentSales = "/admin/tickets/recent-sales";
                public const string AdminUpcomingDraws = "/admin/draws/upcoming";

                // Draw (Sorteios)
                public const string Draws = "/draws";
                public const string DrawById = "/draw/{0}"; // Formato: /draw/guid
                public const string CreateDraw = "/rifa/{0}/draw"; // Formato: /rifa/guid/draw

                // Draw Management
                public const string ExecuteDraw = "/admin/draws/execute/{0}"; // Formato: /admin/draws/execute/guid
                public const string DrawHistory = "/admin/draws/history";
                public const string DrawPreview = "/admin/draws/preview/{0}"; // Formato: /admin/draws/preview/guid
                public const string ScheduleDraw = "/admin/draws/schedule";
                public const string CancelDraw = "/admin/draws/cancel/{0}"; // Formato: /admin/draws/cancel/guid

                // Extra Numbers
                public const string ExtraNumbersByRifa = "/rifas/{0}/extra-numbers"; // Formato: /rifas/guid/extra-numbers
                public const string AddExtraNumber = "/rifas/{0}/extra-number"; // Formato: /rifas/guid/extra-number

                // Unpaid Rifas
                public const string UnpaidRifas = "/unpaidrifas";

                // Webhooks
                public const string MercadoPagoWebhook = "/webhooks/mercadopago";

                // Adicionar na seção de Auth/Users em AppConfig.cs
                public const string UserById = "/manage/users/{0}"; // Para operações em um usuário específico
                public const string UpdateUserRole = "/manage/users/{0}/role"; // Para atualizar o papel de um usuário
                public const string DeleteUser = "/manage/users/{0}"; // Para DELETE de um usuário
                public const string UpdateUser = "/manage/users/{0}"; // Para UPDATE de um usuário

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