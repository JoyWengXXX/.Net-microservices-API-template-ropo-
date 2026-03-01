using System.Linq.Expressions;
using LogInService.Cmd.Domain.Events;

namespace LogInService.Tests
{
    public class EventHandlerTests
    {
        private readonly Mock<IRepository<MainDBConnectionManager>> mockRepo;
        private readonly Mock<IJwtHelper> mockJwtHelper;
        private readonly Mock<IConnectionMultiplexer> mockRedisConnection;
        private readonly Mock<IHttpClientRequestHelper> mockHttpClientRequestHelper;
        private readonly Mock<ILogger<Cmd.Infrastructure.Handlers.EventHandler>> mockLogger;
        private readonly Cmd.Infrastructure.Handlers.EventHandler eventHandler;

        public EventHandlerTests()
        {
            mockRepo = new Mock<IRepository<MainDBConnectionManager>>();
            mockJwtHelper = new Mock<IJwtHelper>();
            mockRedisConnection = new Mock<IConnectionMultiplexer>();
            mockHttpClientRequestHelper = new Mock<IHttpClientRequestHelper>();
            mockLogger = new Mock<ILogger<Cmd.Infrastructure.Handlers.EventHandler>>();
            eventHandler = new Cmd.Infrastructure.Handlers.EventHandler(mockRepo.Object, mockJwtHelper.Object, mockRedisConnection.Object, mockHttpClientRequestHelper.Object, mockLogger.Object);
        }

        [Fact]
        public async Task On_LogInEvent_SuccessfulLogin_ReturnsTResultWithSuccessAndToken()
        {
            var userInfo = new UserInfo { UserId= "DefaultAdmin" };
            var token = "testToken";
            mockRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserInfo, object>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync(userInfo);
            mockJwtHelper.Setup(x => x.GenerateToken(new TokenUserInfo()
            {
                userId = It.IsAny<string>(),
                role = It.IsAny<string>(),
                roleOrder = It.IsAny<int>(),
                photoUrl = It.IsAny<string>(),
                timeZoneId = It.IsAny<string>(),
                goodsIsVisibleSwitch = It.IsAny<bool>()
            })).Returns(token);

            // Act
            var result = await eventHandler.On(new LogInEvent { userAccount = "DefaultAdmin", password = "password" });

            // Assert
            Assert.True(result.isSuccess);
            Assert.Equal(token, result.executionData);
        }

        [Fact]
        public async Task On_LogInEvent_UnsuccessfulLogin_ThrowsAppException()
        {
            // Arrange
            _ = mockRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserInfo, object>>>(), It.IsAny<Expression<Func<UserInfo, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync((UserInfo?)null);

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => eventHandler.On(new LogInEvent { userAccount = "test", password = "password" }));
        }
    }
}

