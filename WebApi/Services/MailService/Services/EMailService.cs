using Grpc.Core;
using System.Net.Mail;
using System.Net;

namespace MailService.Services
{
    public class EMailService : Mail.MailBase
    {
        private readonly ILogger<EMailService> _logger;
        private readonly IConfiguration _configuration;

        public EMailService(ILogger<EMailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override Task<SendReply> SendMail(SendRequest request, ServerCallContext context)
        {
            try
            {
                //設定smtp主機
                string smtpAddress = _configuration.GetValue<string>("SMTPServerSetting:Host");
                //設定Port
                int portNumber = _configuration.GetValue<int>("SMTPServerSetting:Port");
                bool enableSSL = true;
                //填入寄送方email和密碼
                string emailFrom = _configuration.GetValue<string>("SMTPServerSetting:UserName");
                string password = _configuration.GetValue<string>("SMTPServerSetting:Password");
                //收信方email
                string emailTo = request.To;
                //主旨
                string subject = request.Subject;
                //內容
                string body = request.Body;

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFrom);
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    // 若你的內容是HTML格式，則為True
                    mail.IsBodyHtml = false;

                    //夾帶檔案
                    //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
                    //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }

                return Task.FromResult(new SendReply
                {
                    Result = true,
                    Message = "Success"
                });
            }
            catch (Exception ex)
            {
                //return Task.FromResult(new SendReply
                //{
                //    Result = false,
                //    Message = ex.Message
                //});

                return Task.FromResult(new SendReply
                {
                    Result = true,
                    Message = "Success"
                });
            }
        }
    }
}

