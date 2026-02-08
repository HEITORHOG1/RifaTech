namespace RifaTech.API.Entities.Notifications
{
    public class DrawReminderNotification : NotificationBase
    {
        public string ClienteName { get; set; }
        public string RifaName { get; set; }
        public Guid RifaId { get; set; }
        public List<int> TicketNumbers { get; set; }
        public DateTime DrawDateTime { get; set; }
        public TimeSpan TimeRemaining { get; set; }

        public DrawReminderNotification()
        {
            NotificationType = NotificationType.DrawReminder;
            Subject = "Lembrete de Sorteio - RifaTech";
        }
    }
}