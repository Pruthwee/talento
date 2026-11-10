using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// Replaces System.Net.Mail.SmtpClient with Amazon Simple Email Service (SES).
    /// Provides the same sending interface while using AWS SES as the email backend.
    /// </summary>
    public static class SESHelper
    {
        private static IAmazonSimpleEmailService _sesClient;
        private static string _fromAddress;

        static SESHelper()
        {
            var region = ConfigurationManager.AppSettings["AWS:Region"] ?? "us-east-1";
            _fromAddress = ConfigurationManager.AppSettings["AWS:SES:FromAddress"]
                ?? ConfigurationManager.AppSettings["EmailFrom"]
                ?? "noreply@example.com";
            _sesClient = new AmazonSimpleEmailServiceClient(
                RegionEndpoint.GetBySystemName(region));
        }

        /// <summary>
        /// Sends an email using Amazon SES, replacing SmtpClient.SendMail.
        /// </summary>
        public static void SendEmail(string toEmail, string subject, string body, bool isBodyHtml = false)
        {
            var request = new SendEmailRequest
            {
                Source = _fromAddress,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { toEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = isBodyHtml
                        ? new Body { Html = new Content(body) }
                        : new Body { Text = new Content(body) }
                }
            };

            var configurationSet = ConfigurationManager.AppSettings["AWS:SES:ConfigurationSet"];
            if (!string.IsNullOrEmpty(configurationSet))
            {
                request.ConfigurationSetName = configurationSet;
            }

            _sesClient.SendEmail(request);
        }

        /// <summary>
        /// Sends an email from a MailMessage using Amazon SES.
        /// </summary>
        public static void SendMailMessage(MailMessage message)
        {
            var destination = new Destination();
            foreach (var addr in message.To)
            {
                destination.ToAddresses.Add(addr.Address);
            }
            foreach (var addr in message.CC)
            {
                destination.CcAddresses.Add(addr.Address);
            }
            foreach (var addr in message.Bcc)
            {
                destination.BccAddresses.Add(addr.Address);
            }

            var body = message.IsBodyHtml
                ? new Body { Html = new Content(message.Body) }
                : new Body { Text = new Content(message.Body) };

            var request = new SendEmailRequest
            {
                Source = message.From.Address,
                Destination = destination,
                Message = new Message
                {
                    Subject = new Content(message.Subject),
                    Body = body
                }
            };

            var configurationSet = ConfigurationManager.AppSettings["AWS:SES:ConfigurationSet"];
            if (!string.IsNullOrEmpty(configurationSet))
            {
                request.ConfigurationSetName = configurationSet;
            }

            _sesClient.SendEmail(request);
        }
    }
}
