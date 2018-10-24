using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Galvarino.Web.Services.Notification
{
    public class MailSender : INotificationKernel
    {
        
        private SmtpClient client;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailSender> _logger;
        public MailSender(IHostingEnvironment env, IConfiguration configuration, ILogger<MailSender> logger)
        {
            _env = env;
            _configuration = configuration;
            _logger = logger;
        }

        private string setTemplate(string name)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Path.Combine(_env.ContentRootPath, "wwwroot", "template","Default.html")))
            {
                body = reader.ReadToEnd();
            }
            return body.Replace("{texto_a_mostrar_dinamico}", name);
        }

        public void Send(string to, string template)
        {
            client = new SmtpClient("smtp.gmail.com");
            client.EnableSsl = true;
            client.Port = 465;
            client.Credentials = new NetworkCredential("robotic.carlos@gmail.com", "mineros_388");
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("galvarinoNR@laaraucana.cl");
            mailMessage.To.Add(to);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = this.setTemplate(template);
            mailMessage.Subject = "Hola Mundo";
            client.Send(mailMessage);
        }

        public async Task SendEmail(string email, string subject, string message)
        {
            _logger.LogDebug("preparando el envio de correo");
            using (var client = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = _configuration["Email:Email"],
                    Password = _configuration["Email:Password"]
                };

                client.Credentials = credential;
                client.Host = _configuration["Email:Host"];
                client.Port = int.Parse(_configuration["Email:Port"]);
                client.EnableSsl = true;

                using (var emailMessage = new MailMessage())
                {
                    emailMessage.To.Add(new MailAddress(email));
                    emailMessage.From = new MailAddress(_configuration["Email:From"]);
                    emailMessage.Subject = subject;
                    emailMessage.Body = this.setTemplate(message);
                    emailMessage.IsBodyHtml=true;
                    client.Send(emailMessage);
                    _logger.LogDebug("mandamos el correo");
                }
            }
            await Task.CompletedTask;
        }

        
    }
}
