using System.Linq.Expressions;

namespace ControllerService.Tests
{
    public class QueryHandlerTests
    {
        private readonly Mock<IRepository<MainDBConnectionManager>> mockRepo;
        private readonly Query.Infrastructure.Handlers.QueryHandler queryHandler;

        public QueryHandlerTests()
        {
            mockRepo = new Mock<IRepository<MainDBConnectionManager>>();
            queryHandler = new Query.Infrastructure.Handlers.QueryHandler(mockRepo.Object);
        }

        [Fact]
        public async Task On_GetPagesQuery_ReturnsTResultWithSuccessAndResults()
        {
            // Arrange
            var mockedResultObject = new List<Controller>() 
            {
                new Controller() { ControllerId = "TEST_Function", ControllerName = "Admin", IsEnable = true },
                new Controller() { ControllerId = "TEST_Function", ControllerName = "User", IsEnable = true },
                new Controller() { ControllerId = "TEST_Function", ControllerName = "Disabled", IsEnable = false }
            };
            mockRepo.Setup(x => x.GetListAsync(It.IsAny<Expression<Func<Controller, object>>>(), It.IsAny<Expression<Func<Controller, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync(mockedResultObject);

            // Act
            var result = await queryHandler.HandleAsync(new ControllerService.Query.Domain.Queries.GetControllersQuery());

            // Assert
            Assert.Equal(mockedResultObject, result.executionData);
        }
    }
}

