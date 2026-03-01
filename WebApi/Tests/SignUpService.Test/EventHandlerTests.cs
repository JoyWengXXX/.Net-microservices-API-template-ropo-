using CommonLibrary.Helpers.Interfaces;
using SignUpService.Cmd.Domain.Events;
using System.Linq.Expressions;
using System.Data;

namespace SignUpService.Tests
{
    public class EventHandlerTests
    {
        private readonly Mock<IRepository<MainDBConnectionManager>> mockMainRepo;
        private readonly Cmd.Infrastructure.Handlers.EventHandler _eventHandler;

        public EventHandlerTests()
        {
            mockMainRepo = new Mock<IRepository<MainDBConnectionManager>>();
            var mockIKeyGeneratorHelper = new Mock<IKeyGeneratorHelper>();
            var mockIValidateHelper = new Mock<IValidateHelper>();
            var mockIHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockIHttpClientRequestHelper = new Mock<IHttpClientRequestHelper>();
            _eventHandler = new Cmd.Infrastructure.Handlers.EventHandler(mockMainRepo.Object, mockIKeyGeneratorHelper.Object, mockIValidateHelper.Object, mockIHttpContextAccessor.Object, mockIHttpClientRequestHelper.Object);
        }

        [Fact]
        public async Task On_SignUpEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var signInEvent = new SignUpEvent
            {
                userId = "testUser",
                password = "testPassword",
            };
            mockMainRepo.Setup(x => x.GetConnection()).Returns(new Mock<IDbConnection>().Object);
            mockMainRepo.Setup(x => x.GetConnection().BeginTransaction()).Returns(new Mock<IDbTransaction>().Object);
            mockMainRepo.Setup(x => x.CreateAsync(It.IsAny<UserInfo>(), It.IsAny<IUnitOfWork>())).ReturnsAsync(1);

            // Act
            var result = await _eventHandler.On(signInEvent);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_ValidEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var @event = new ValidateEvent
            {
                userId = "testUser",
                validationCode = "123456"
            };
            var user = new UserInfo
            {
                UserId = "testUser",
                VerificationCode = "123456"
            };
            mockMainRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserInfo, object>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), null, It.IsAny<IUnitOfWork>()))
                .ReturnsAsync(user);
            mockMainRepo.Setup(x => x.UpdateAsync(It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<IUnitOfWork>()))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(@event);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_ForgetPasswordEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var forgetPasswordEvent = new ForgetPasswordEvent
            {
                userId = "test@email.com"
            };
            var user = new UserInfo
            {
                UserId = "test@email.com",
                IsVerify = true
            };
            mockMainRepo.Setup(x => x.GetConnection()).Returns(new Mock<IDbConnection>().Object);
            mockMainRepo.Setup(x => x.GetConnection().BeginTransaction()).Returns(new Mock<IDbTransaction>().Object);
            mockMainRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserInfo, object>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), null, It.IsAny<IUnitOfWork>()))
                .ReturnsAsync(user);
            mockMainRepo.Setup(x => x.UpdateAsync(It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<IUnitOfWork>()))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(forgetPasswordEvent);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_PasswordChangeEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var passwordChangeEvent = new PasswordChangeEvent
            {
                EventOperator = "testUser",
                newPassword = "newPassword"
            };
            var user = new UserInfo
            {
                UserId = "test@email.com",
                IsVerify = true,
            };
            mockMainRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserInfo, object>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), null, It.IsAny<IUnitOfWork>()))
                .ReturnsAsync(user);
            mockMainRepo.Setup(x => x.UpdateAsync(It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<IUnitOfWork>()))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(passwordChangeEvent);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_AccountDisableEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var accountDisableEvent = new AccountDisableEvent
            {
                userId = "test@email.com"
            };
            var user = new UserInfo
            {
                UserId = "test@email.com"
            };
            mockMainRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserInfo, object>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), null, It.IsAny<IUnitOfWork>()))
                .ReturnsAsync(user);
            mockMainRepo.Setup(x => x.UpdateAsync(It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), It.IsAny<IUnitOfWork>()))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(accountDisableEvent);

            // Assert
            Assert.True(result.isSuccess);
        }
    }
}

