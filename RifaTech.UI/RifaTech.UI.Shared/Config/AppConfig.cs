using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.UI.Shared.Config
{
    public static class AppConfig
    {
        // URLs da API
        public static class Api
        {
            public const string BaseUrl = "https://localhost:7212";

            // Endpoints específicos
            public static class Endpoints
            {
                public const string Rifas = "/api/rifas";
                public const string Tickets = "/api/tickets";
                public const string Clientes = "/api/clientes";
                public const string Pagamentos = "/api/payments";
                public const string CompraRapida = "/api/compra-rapida";
                public const string Auth = "/api/manage";
            }
        }

        // Configurações de cache e armazenamento local
        public static class LocalStorage
        {
            public const string AuthTokenKey = "authToken";
            public const string RefreshTokenKey = "refreshToken";
            public const string UserEmailKey = "email";
        }

        // Outras configurações globais
        public static class Settings
        {
            public const int DefaultPageSize = 12;
            public const int PaymentExpirationMinutes = 30;
            public const int AutoRefreshIntervalSeconds = 10;
        }
    }
}
