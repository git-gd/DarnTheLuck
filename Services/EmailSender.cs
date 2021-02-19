using DarnTheLuck.Helpers;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace WebPWrecover.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailjetClient client = new MailjetClient(Ring.MailjetKey1, Ring.MailjetKey2);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
               .Property(Send.FromEmail, Ring.FromEmail)
               .Property(Send.FromName, "DTL")
               .Property(Send.Subject, subject)
               .Property(Send.TextPart, htmlMessage)
               .Property(Send.HtmlPart, htmlMessage)
               .Property(Send.Recipients, new JArray { new JObject { {"Email", email} }
            });

            return client.PostAsync(request);
        }
    }
}