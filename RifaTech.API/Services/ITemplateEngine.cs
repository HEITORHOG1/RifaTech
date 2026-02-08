using RifaTech.API.Entities.Notifications;

namespace RifaTech.API.Services
{
    public interface ITemplateEngine
    {
        string RenderTemplate(string templateName, NotificationBase model);
    }
}