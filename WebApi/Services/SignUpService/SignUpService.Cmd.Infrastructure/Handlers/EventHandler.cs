using static Service.Common.Models.UserInfoEnums;
using static Service.Common.Models.UserRoleEnums;
using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using SignUpService.Cmd.Domain.Handlers;
using System.Text;
using Service.Common.Middleware;
using CommonLibrary.Helpers;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using SignUpService.Cmd.Domain.Models;
using Service.Common.Models;
using CommonLibrary.Helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using SignUpService.Cmd.Domain.Events;
using System.Linq.Expressions;
using LinqKit;
using Service.Common.Helpers.Interfaces;

namespace SignUpService.Cmd.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repoMain;
        private readonly IKeyGeneratorHelper _keyGeneratorHelper;
        private readonly IValidateHelper _validateHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IHttpClientRequestHelper _httpClientRequestHelper;

        private readonly string AESKey;
        private readonly string webPath;
        private readonly string SMTPHost;
        private readonly int SMTPPort;
        private readonly string SMTPUserAccount;
        private readonly string SMTPUserPW;

        public EventHandler(IRepository<MainDBConnectionManager> repoMain,
                            IKeyGeneratorHelper keyGeneratorHelper,
                            IValidateHelper validateHelper,
                            IHttpContextAccessor httpContextAccessor,
                            IHttpClientRequestHelper httpClientRequestHelper)
        {
            _repoMain = repoMain;
            _keyGeneratorHelper = keyGeneratorHelper;
            _validateHelper = validateHelper;
            _httpContextAccessor = httpContextAccessor;
            _httpClientRequestHelper = httpClientRequestHelper;

            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var sysConfig = ConfigurationHelper.LoadConfiguration(
                directoryInfo.FullName,
                "CommonSettings"
            );
            AESKey = sysConfig.GetValue<string>("SystemConfig:AESEncryptKey");
            webPath = sysConfig.GetValue<string>("SystemConfig:WebPath");
            SMTPHost = sysConfig.GetValue<string>("SMTPServerSetting:Host");
            SMTPPort = sysConfig.GetValue<int>("SMTPServerSetting:Port");
            SMTPUserAccount = sysConfig.GetValue<string>("SMTPServerSetting:UserAccount");
            SMTPUserPW = sysConfig.GetValue<string>("SMTPServerSetting:Password");
        }

        public async Task<TResult> On(SignUpEvent @event)
        {
            using var mainUOW = _repoMain.CreateUnitOfWork();
            var existedUser = await _repoMain.GetFirstAsync<UserInfo>(x => new { x.UserId }, x => x.UserId == @event.userId, unitOfWork: mainUOW);
            if (existedUser != null) { throw new AppException("Duplicate userId!", ReturnResultCodeEnums.SystemResultCode.A03); }
            if (!_validateHelper.ValidatePassword(@event.password)) { throw new AppException("Invalid password form!", code: ReturnResultCodeEnums.SystemResultCode.InvalidInputForm, isSuccess: true, httpStatusCode: StatusCodes.Status200OK); }
            string validationCode = _keyGeneratorHelper.GenerateRandomCapitalLetters();
            mainUOW.Begin();
            try
            {
                var userId = await _repoMain.CreateAsync(new UserInfo
                {
                    UserId = @event.userId,
                    Password = Convert.ToBase64String(EncryptDecryptHelper.Instance.CreateHash(@event.password, EncryptDecryptHelper.HashType.SHA256)).Replace("-", ""),
                    Name = @event.name,
                    IsVerify = false,
                    VerificationCode = validationCode,
                    UserStatus = (int)UserStatus.Disabled,
                    SSOType = (int)SSOType.RegularLogin,
                    TimeZoneID = "Asia/Taipei",
                    CreateDate = DateTime.UtcNow
                }, mainUOW);
                var redirectValidationParams = EncryptDecryptHelper.Instance.AESEncrypt(JsonSerializer.Serialize(new ValidateParameters { userId = @event.userId, validationCode = validationCode }), AESKey);
                if (redirectValidationParams == null || redirectValidationParams == string.Empty) { throw new AppException("validation url fail!"); }
                await SendValidationCode("雲端掌櫃帳號驗證", $"您的驗證: <a href=\"{webPath}/validation?input={redirectValidationParams}\">請點擊此連結</a>", @event.userId);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult
            {
                isSuccess = true,
                executionData = "Please complete validation."
            };
        }

        public async Task<TResult> On(ValidateEvent @event)
        {
            using var mainUOW = _repoMain.CreateUnitOfWork();
            string UserId = @event.userId;
            var user = await _repoMain.GetFirstAsync<UserInfo>(x => new { x.UserId, x.VerificationCode }, x => x.UserId == UserId && x.IsVerify == false, unitOfWork: mainUOW);
            if (user == null)
            {
                throw new AppException("User not found", ReturnResultCodeEnums.SystemResultCode.A01);
            }
            if (user.VerificationCode != @event.validationCode)
            {
                throw new AppException("Validation code is incorrect", ReturnResultCodeEnums.SystemResultCode.A04);
            }
            mainUOW.Begin();
            try
            {
                await _repoMain.UpdateAsync<UserInfo>(x => x.IsVerify == true &&
                                                           x.VerificationDate == DateTime.UtcNow &&
                                                           x.UserStatus == (int)UserStatus.Active &&
                                                           x.UpdateDate == DateTime.UtcNow,
                                                      x => x.UserId == user.UserId,
                                                      mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(ForgetPasswordEvent @event)
        {
            using var mainUOW = _repoMain.CreateUnitOfWork();
            string UserId = @event.userId;
            var user = await _repoMain.GetFirstAsync<UserInfo>(x => new { x.UserId, x.IsVerify }, x => x.UserId == UserId, unitOfWork: mainUOW);
            if (user == null)
            {
                throw new AppException("User not found", ReturnResultCodeEnums.SystemResultCode.A01);
            }
            if (!user.IsVerify)
            {
                throw new AppException("User is not verified", ReturnResultCodeEnums.SystemResultCode.A06);
            }
            var random = new Random();
            string tempPassword = _keyGeneratorHelper.GenerateRandString(random.Next(8, 12), true, false);

            mainUOW.Begin();
            try
            {
                await _repoMain.UpdateAsync<UserInfo>(x => x.Password == Convert.ToBase64String(EncryptDecryptHelper.Instance.CreateHash(tempPassword, EncryptDecryptHelper.HashType.SHA256)).Replace("-", "") &&
                                                           x.UpdateDate == DateTime.UtcNow, 
                                                      x => x.UserId == UserId,
                                                      mainUOW);
                await SendValidationCode("Forget password", $"Your temporary password is {tempPassword}", @event.userId);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult
            {
                isSuccess = true
            };
        }

        public async Task<TResult> On(PasswordChangeEvent @event)
        {
            using var mainUOW = _repoMain.CreateUnitOfWork();
            string UserId = @event.EventOperator;
            var user = await _repoMain.GetFirstAsync<UserInfo>(x => new { x.UserId, x.IsVerify, x.SSOType }, x => x.UserId == UserId, unitOfWork: mainUOW);
            if (user == null)
            {
                throw new AppException("User not found", ReturnResultCodeEnums.SystemResultCode.A01);
            }
            if (!user.IsVerify)
            {
                throw new AppException("User is not verified", ReturnResultCodeEnums.SystemResultCode.A06);
            }
            if (user.SSOType != (int)SSOType.RegularLogin)
            {
                throw new AppException("SSO login account can't modify password!", ReturnResultCodeEnums.SystemResultCode.A08);
            }
            if (!_validateHelper.ValidatePassword(@event.newPassword))
            {
                throw new AppException("Invalid password form!", code: ReturnResultCodeEnums.SystemResultCode.A05);
            }

            mainUOW.Begin();
            try
            {
                await _repoMain.UpdateAsync<UserInfo>(x => x.Password == Convert.ToBase64String(EncryptDecryptHelper.Instance.CreateHash(@event.newPassword, EncryptDecryptHelper.HashType.SHA256)).Replace("-", "") &&
                                                           x.UpdateDate == DateTime.UtcNow, 
                                                      x => x.UserId == user.UserId,
                                                      mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(UserInfoChangeEvent @event)
        {
            using var mainUOW = _repoMain.CreateUnitOfWork();
            string UserId = @event.EventOperator;
            var user = await _repoMain.GetFirstAsync<UserInfo>(x => new { x.UserId, x.IsVerify }, x => x.UserId == UserId, unitOfWork: mainUOW);
            if (user == null)
            {
                throw new AppException("User not found", ReturnResultCodeEnums.SystemResultCode.A01);
            }
            if (!user.IsVerify)
            {
                throw new AppException("User is not verified", ReturnResultCodeEnums.SystemResultCode.A06);
            }

            mainUOW.Begin();
            try
            {
                Expression<Func<UserInfo, bool>> updateExpression = x => x.UpdateDate == DateTime.UtcNow;
                if (!string.IsNullOrEmpty(@event.input.userName))
                {
                    updateExpression = updateExpression.And(x => x.Name == @event.input.userName);
                }
                if (@event.input.timeZoneId != null)
                {
                    updateExpression = updateExpression.And(x => x.TimeZoneID == @event.input.timeZoneId);
                }
                await _repoMain.UpdateAsync<UserInfo>(updateExpression,
                                                      x => x.UserId == user.UserId,
                                                      mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(AccountDisableEvent @event)
        {
            using var mainUOW = _repoMain.CreateUnitOfWork();
            int userStatus = (int)UserStatus.Disabled;
            string UserId = @event.userId;
            var user = await _repoMain.GetFirstAsync<UserInfo>(x => new { x.UserId }, x => x.UserId == UserId && x.UserStatus == userStatus, unitOfWork: mainUOW);
            if (user == null)
            {
                throw new AppException("User not found", ReturnResultCodeEnums.SystemResultCode.A01);
            }
            userStatus = (int)UserStatus.Disabled;

            mainUOW.Begin();
            try
            {
                await _repoMain.UpdateAsync<UserInfo>(x => x.UserStatus == userStatus && x.UpdateDate == DateTime.UtcNow, 
                                                      x => x.UserId == user.UserId,
                                                      mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(AccountDeleteEvent @event)
        {
            using var mainUOW = _repoMain.CreateUnitOfWork();

            var user = await _repoMain.GetFirstAsync<UserInfo>(x => new { x.IsAdmin }, x => x.UserId == @event.userId, unitOfWork: mainUOW);
            if (user == null) { throw new AppException("User not found!", ReturnResultCodeEnums.SystemResultCode.A01); }
            if (user.IsAdmin) { throw new AppException("Can't delete admin user!", ReturnResultCodeEnums.SystemResultCode.InvalidOperation); }

            var ownerRoleId = await _repoMain.GetFirstAsync<UserRole>(x => new { x.RoleId }, x => x.RoleOrder == (int)Role.FirstRankUser, unitOfWork: mainUOW);
           
            mainUOW.Begin();
            try
            {
                await _repoMain.DeleteAsync<UserInfo>(x => x.UserId == @event.userId, mainUOW);
                await _repoMain.DeleteAsync<LogInRecord>(x => x.UserId == @event.userId, mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }


        private async Task SendValidationCode(string subjectStr, string bodyStr, string sendToStr)
        {
            //設定smtp主機
            string smtpAddress = SMTPHost;
            //設定Port
            int portNumber = SMTPPort;
            bool enableSSL = true;
            //填入寄送方email和密碼
            string emailFrom = SMTPUserAccount;
            string password = SMTPUserPW;
            //收信方email
            string emailTo = sendToStr;
            //主旨
            string subject = subjectStr;
            //內容
            string body = bodyStr;
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                mail.SubjectEncoding = Encoding.UTF8;
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, password);
                    smtp.EnableSsl = enableSSL;
                    smtp.Send(mail);
                }
            }
        }
    }
}


