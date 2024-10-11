using System.Security.Authentication;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using MailKit.Security;
using System.Net;

namespace FoodServiceBackend.Email
{
    public static class EmailHelper
    {
        public static async Task SendEmailAsync(string body, string recipientEmailAddress, string subject, string fileAttachmentPath = "")
        {
            if (Program.settings == null || string.IsNullOrEmpty(recipientEmailAddress))
            {
                return;
            }

            // Define the scope for Gmail
            var scopes = new[] { "https://mail.google.com/" };

            // Authorize and get credentials
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromFile("Assets/secret.json").Secrets,
                scopes,
                Program.settings.SenderEmail,
                CancellationToken.None,
                new FileDataStore(Directory.GetCurrentDirectory(), true)).Result;

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

                        // Send the email
                        try
                        {
                            using (var client = new SmtpClient())
                            {
                                client.Connect("smtp.gmail.com", 465, true);
                                var oauth2 = new SaslMechanismOAuth2(Program.settings.SenderEmail, credential.Token.AccessToken);
                                await client.AuthenticateAsync(oauth2);
                                client.Send(emailMessage);
                                client.Disconnect(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
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
