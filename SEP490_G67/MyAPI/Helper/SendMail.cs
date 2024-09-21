using MyAPI.DTOs;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace MyAPI.Helper
{
    public class SendMail
    {
        public async Task<bool> SendEmail(SendMailDTO sendMailDTO)
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(sendMailDTO.FromEmail);
            mail.To.Add(sendMailDTO.ToEmail);
            mail.Subject = sendMailDTO.Subject;
            mail.Body = sendMailDTO.Body;

            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.Credentials = new NetworkCredential(sendMailDTO.FromEmail, sendMailDTO.Password);
                smtpClient.EnableSsl = true;

                try
                {
                    smtpClient.Send(mail);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public string GenerateVerificationCode(int length)
        {
            const string chars = "0123456789";
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
    }
}
