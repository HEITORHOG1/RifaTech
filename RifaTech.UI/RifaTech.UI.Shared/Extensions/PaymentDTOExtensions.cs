using RifaTech.DTOs.DTOs;
using RifaTech.UI.Shared.Models;

namespace RifaTech.UI.Shared.Extensions
{
    public static class PaymentDTOExtensions
    {
        public static PaymentStatus GetStatus(this PaymentDTO payment)
        {
            // Conversão baseada nos valores existentes
            if (payment.IsConfirmed)
                return PaymentStatus.Confirmed;

            // Verificar se expirou (se tivermos ExpirationTime disponível)
            if (payment.ExpirationTime.HasValue && payment.ExpirationTime.Value < DateTime.UtcNow)
                return PaymentStatus.Expired;

            return PaymentStatus.Pending;
        }
    }

    // Extensão para TicketDTO para adicionar CreatedAt
    public static class TicketDTOExtensions
    {
        public static DateTime GetCreatedAt(this TicketDTO ticket)
        {
            // Se a propriedade GeneratedTime existir, usá-la
            if (ticket.GeneratedTime.HasValue)
                return ticket.GeneratedTime.Value;

            // Caso contrário, retornar data atual
            return DateTime.UtcNow;
        }
    }
}