using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend.Email
{
    public static class EmailHelper
    {
        public static async Task SendEmailAsync(string body, string recipientEmailAddress, string subject, string fileAttachmentPath = "")
        {
            if (Program.settings == null)
            {
                return;
            }

            string ToMailAddress = recipientEmailAddress;
            bool failedSending = false;

            do
            {
                failedSending = false;

                try
                {
                    using (MimeKit.MimeMessage emailMessage = new MimeKit.MimeMessage())
                    {
                        emailMessage.From.Add(new MimeKit.MailboxAddress("", Program.settings.SenderEmail));

                        //Now supports multiple recipant addresses separated by ;
                        if (ToMailAddress.Contains(";"))
                        {
                            string[] toMailAddresses = ToMailAddress.Split(';');
                            emailMessage.To.AddRange(toMailAddresses.Select(x => MimeKit.MailboxAddress.Parse(x.Trim())));
                        }
                        else
                        {
                            emailMessage.To.Add(MimeKit.MailboxAddress.Parse(ToMailAddress.Trim()));
                        }

                        emailMessage.Subject = subject;
                        var bodyBuilder = new MimeKit.BodyBuilder();
                        bodyBuilder.HtmlBody = body;
                        if (fileAttachmentPath != "")
                        {
                            bodyBuilder.Attachments.Add(fileAttachmentPath);
                        }

                        emailMessage.Body = bodyBuilder.ToMessageBody();

                        using (var client = new MailKit.Net.Smtp.SmtpClient())
                        {
                            client.Timeout = 15000;
                            // Accept all SSL certificates (in case the server supports STARTTLS)
                            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                            //Some emailproviders don't seem to hand out
                            //proper signed certificates
                            client.CheckCertificateRevocation = false;
                            client.SslProtocols = SslProtocols.Tls12;

                            await client.ConnectAsync("smtp-mail.outlook.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                            // Note: since we don't have an OAuth2 token, disable
                            // the XOAUTH2 authentication mechanism.
                            client.AuthenticationMechanisms.Remove("XOAUTH2");

                            // Note: only needed if the SMTP server requires authentication
                            await client.AuthenticateAsync(Program.settings.SenderEmail, Program.settings.SenderPassword);

                            await client.SendAsync(emailMessage);
                            await client.DisconnectAsync(true);
                        }
                    }

                }
                catch (Exception ex)
                {
                    failedSending = true;
                    string errorMessage = "Failed to send email to " + recipientEmailAddress + " Error: " + ex.Message;
                    Helper.WriteIntoConsoleAndLog(errorMessage);
                }
            }
            while (failedSending == true);            
        }


    }
}
