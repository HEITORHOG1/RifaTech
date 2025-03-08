namespace RifaTech.API.Entities.Notifications
{
    public class DrawResultNotification : NotificationBase
    {
        public string RifaName { get; set; }
        public Guid RifaId { get; set; }
        public DateTime DrawDateTime { get; set; }
        public int WinningNumber { get; set; }
        public string WinnerName { get; set; }

        public DrawResultNotification()
        {
            NotificationType = NotificationType.DrawResult;
            Subject = "Resultado do Sorteio - RifaTech";
        }
    }
}
