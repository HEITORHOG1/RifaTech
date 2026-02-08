namespace RifaTech.API.Entities.Notifications
{
    /// <summary>
    /// Base abstrata para todos os tipos de notificação
    /// </summary>
    public abstract class NotificationBase
    {
        /// <summary>
        /// Tipo de notificação
        /// </summary>
        public NotificationType NotificationType { get; set; }

        /// <summary>
        /// Destinatário da notificação
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Assunto da notificação
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Data de criação da notificação
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Dados adicionais da notificação no formato chave-valor
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Tipos de notificação suportados
    /// </summary>
    public enum NotificationType
    {
        PurchaseConfirmation,    // Confirmação de compra
        PaymentConfirmation,     // Confirmação de pagamento
        PaymentExpired,          // Pagamento expirado
        DrawReminder,            // Lembrete de sorteio
        DrawResult,              // Resultado do sorteio
        WinnerNotification       // Notificação ao ganhador
    }
}