using System.Text.Json.Serialization;

namespace RifaTech.API.Entities
{
    public class WebhookPaymentNotification
    {
        /// <summary>
        /// Tipo de recurso (payment, subscription, etc.)
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// ID da notificação
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Data da notificação (timestamp)
        /// </summary>
        [JsonPropertyName("date_created")]
        public string DateCreated { get; set; }

        /// <summary>
        /// Application ID
        /// </summary>
        [JsonPropertyName("application_id")]
        public long ApplicationId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Versão da API
        /// </summary>
        [JsonPropertyName("api_version")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Ação (created, updated, etc.)
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>
        /// Dados do recurso
        /// </summary>
        [JsonPropertyName("data")]
        public NotificationData Data { get; set; }
    }

    public class NotificationData
    {
        /// <summary>
        /// ID do recurso (ID do pagamento, assinatura, etc.)
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
