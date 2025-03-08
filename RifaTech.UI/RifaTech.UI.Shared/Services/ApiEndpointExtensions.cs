using RifaTech.UI.Shared.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.UI.Shared.Services
{
    /// <summary>
    /// Extensões para facilitar o uso dos endpoints da API
    /// </summary>
    public static class ApiEndpointExtensions
    {
        /// <summary>
        /// Obtém dados de um endpoint da API
        /// </summary>
        public static async Task<T> GetFromApiAsync<T>(this HttpClient client, string endpoint)
        {
            try
            {
                return await client.GetFromJsonAsync<T>(endpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao acessar a API: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtém uma rifa pelo seu ID
        /// </summary>
        public static async Task<T> GetRifaByIdAsync<T>(this HttpClient client, Guid id)
        {
            string endpoint = string.Format(AppConfig.Api.Endpoints.RifaById, id);
            return await client.GetFromApiAsync<T>(endpoint);
        }

        /// <summary>
        /// Obtém tickets de uma rifa específica
        /// </summary>
        public static async Task<T> GetTicketsByRifaAsync<T>(this HttpClient client, Guid rifaId)
        {
            string endpoint = string.Format(AppConfig.Api.Endpoints.TicketsByRifa, rifaId);
            return await client.GetFromApiAsync<T>(endpoint);
        }

        /// <summary>
        /// Busca cliente por email, telefone ou CPF
        /// </summary>
        public static async Task<T> LookupClienteAsync<T>(this HttpClient client, string email, string phoneNumber, string cpf)
        {
            string endpoint = $"{AppConfig.Api.Endpoints.ClienteLookup}?email={Uri.EscapeDataString(email)}&phoneNumber={Uri.EscapeDataString(phoneNumber)}&cpf={Uri.EscapeDataString(cpf)}";
            return await client.GetFromApiAsync<T>(endpoint);
        }

        /// <summary>
        /// Verifica status de um pagamento
        /// </summary>
        public static async Task<T> CheckPaymentStatusAsync<T>(this HttpClient client, Guid paymentId)
        {
            string endpoint = string.Format(AppConfig.Api.Endpoints.PaymentStatus, paymentId);
            return await client.GetFromApiAsync<T>(endpoint);
        }

        /// <summary>
        /// Formata uma string de moeda usando a configuração padrão
        /// </summary>
        public static string FormatCurrency(this decimal value)
        {
            return value.ToString(AppConfig.Formatting.CurrencyFormat,
                System.Globalization.CultureInfo.GetCultureInfo(AppConfig.Formatting.CurrencyCulture));
        }

        /// <summary>
        /// Formata uma data usando a configuração padrão
        /// </summary>
        public static string FormatDate(this DateTime date)
        {
            return date.ToString(AppConfig.Formatting.DateFormat);
        }

        /// <summary>
        /// Formata uma data e hora usando a configuração padrão
        /// </summary>
        public static string FormatDateTime(this DateTime dateTime)
        {
            return dateTime.ToString(AppConfig.Formatting.DateTimeFormat);
        }
    }
}
