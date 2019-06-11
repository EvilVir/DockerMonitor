using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StrangeFog.Docker.Monitor.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StrangeFog.Docker.Monitor.Plugins.Mailer
{
    public class ActionableStateDetectedEventHandler : IEventHandler<ActionableStateDetectedEvent>
    {
        protected readonly ILogger logger;
        protected static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() } },
        };

        public ActionableStateDetectedEventHandler(ILogger<ActionableStateDetectedEventHandler> logger)
        {
            this.logger = logger;
        }

        public async Task HandleAsync(ActionableStateDetectedEvent @event)
        {
            using (logger.BeginScope())
            {
                logger.LogDebug(10000, $"Received {nameof(ActionableStateDetectedEvent)}");

                try
                {
                    SmtpClient client = null;

                    foreach (var state in @event.GroupStates)
                    {
                        foreach (var kvp in Mailer.Config.Actions)
                        {
                            if (kvp.Key.HasFlag(state))
                            {
                                logger.LogInformation(10001, "Sending e-mails about detected states [{States}] of group [{Group}]", @event.GroupStates, @event.Group);

                                if (client == null)
                                {
                                    client = CreateClient();
                                    logger.LogDebug(10005, "SMTP client created");
                                }

                                var message = ComposeMessage(kvp.Value, @event);
                                logger.LogDebug(10004, "Mail message composed");

                                await client.SendMailAsync(message);
                                logger.LogInformation(10002, "E-mails sent");
                            }
                        }
                    }

                    if (client != null)
                    {
                        client.Dispose();
                    }
                }
                catch(Exception e)
                {
                    logger.LogError(10003, "Error {ExceptionType} while sending e-mails: {Message}", e.GetType(), e.Message);
                }
            }
        }

        protected SmtpClient CreateClient()
        {
            return new SmtpClient(Mailer.Config.Server.Host)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Mailer.Config.Server.User, Mailer.Config.Server.Password),
                EnableSsl = Mailer.Config.Server.SSL,
                Port = Mailer.Config.Server.Port ?? (Mailer.Config.Server.SSL ? 465 : 25),
                Timeout = 30
            };
        }

        protected MailMessage ComposeMessage(Configuration.Action action, ActionableStateDetectedEvent @event)
        {
            var subject = RenderTemplate(action.Subject, @event);
            var body = RenderTemplate(action.Message, @event);
            var output = new MailMessage(Mailer.Config.Server.From, Mailer.Config.Server.From, subject, body);

            foreach (var bcc in action.Recipients)
            {
                output.Bcc.Add(bcc);
            }

            return output;
        }

        protected string RenderTemplate(string template, ActionableStateDetectedEvent @event)
        {
            foreach (var prop in @event.GetType().GetProperties())
            {
                var value = JsonConvert.SerializeObject(prop.GetValue(@event), Formatting.Indented, serializerSettings);
                template = template.Replace("{" + prop.Name + "}", value);
            }

            return template;
        }
    }
}
