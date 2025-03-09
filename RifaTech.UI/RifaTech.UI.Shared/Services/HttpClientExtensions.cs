using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RifaTech.UI.Shared.Services
{
    // Tornamos a classe estática
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Ajusta um endpoint da API para o formato correto, removendo o prefixo '/api' se necessário
        /// </summary>
        public static string AdjustEndpoint(this HttpClient client, string endpoint)
        {
            // Se o endpoint começar com "/api/", remover o "/api"
            if (endpoint.StartsWith("/api/"))
            {
                return endpoint.Substring(4);
            }
            return endpoint;
        }

        /// <summary>
        /// Versão ajustada de GetFromJsonAsync que corrige automaticamente os endpoints
        /// </summary>
        public static async Task<T> GetFromJsonAsyncAdjusted<T>(this HttpClient client, string endpoint, CancellationToken cancellationToken = default)
        {
            string adjustedEndpoint = client.AdjustEndpoint(endpoint);
            return await client.GetFromJsonAsync<T>(adjustedEndpoint, cancellationToken);
        }

        /// <summary>
        /// Versão ajustada de PostAsJsonAsync que corrige automaticamente os endpoints
        /// </summary>
        public static async Task<HttpResponseMessage> PostAsJsonAsyncAdjusted<T>(this HttpClient client, string endpoint, T value, CancellationToken cancellationToken = default)
        {
            string adjustedEndpoint = client.AdjustEndpoint(endpoint);
            return await client.PostAsJsonAsync(adjustedEndpoint, value, cancellationToken);
        }

        /// <summary>
        /// Versão ajustada de DeleteAsync que corrige automaticamente os endpoints
        /// </summary>
        public static async Task<HttpResponseMessage> DeleteAsyncAdjusted(this HttpClient client, string endpoint, CancellationToken cancellationToken = default)
        {
            string adjustedEndpoint = client.AdjustEndpoint(endpoint);
            return await client.DeleteAsync(adjustedEndpoint, cancellationToken);
        }

        /// <summary>
        /// Versão ajustada de PutAsJsonAsync que corrige automaticamente os endpoints
        /// </summary>
        public static async Task<HttpResponseMessage> PutAsJsonAsyncAdjusted<T>(this HttpClient client, string endpoint, T value, CancellationToken cancellationToken = default)
        {
            string adjustedEndpoint = client.AdjustEndpoint(endpoint);
            return await client.PutAsJsonAsync(adjustedEndpoint, value, cancellationToken);
        }
    }
}
