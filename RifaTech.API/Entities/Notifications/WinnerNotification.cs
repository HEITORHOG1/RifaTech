namespace RifaTech.API.Entities.Notifications
{
    public class WinnerNotification : NotificationBase
    {
        public string ClienteName { get; set; }
        public string RifaName { get; set; }
        public Guid RifaId { get; set; }
        public int WinningNumber { get; set; }
        public decimal PrizeValue { get; set; }
        public string ContactInfo { get; set; }

        public WinnerNotification()
        {
            NotificationType = NotificationType.WinnerNotification;
            Subject = "Parabéns! Você ganhou - RifaTech";
        }
    }
}