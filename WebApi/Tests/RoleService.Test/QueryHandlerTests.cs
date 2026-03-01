using System.Linq.Expressions;

namespace RoleService.Tests
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
        public async Task On_GetRolesQuery_ReturnsTResultWithSuccessAndResults()
        {
            // Arrange
            var mockedResultObject = new List<UserRole>() 
            {
                new UserRole() { RoleId = Guid.NewGuid(), RoleName = "Admin", IsEnable = true },
                new UserRole() { RoleId = Guid.NewGuid(), RoleName = "User", IsEnable = true },
                new UserRole() { RoleId = Guid.NewGuid(), RoleName = "Disabled", IsEnable = false }
            };
            mockRepo.Setup(x => x.GetListAsync(It.IsAny<Expression<Func<UserRole, object>>>(), It.IsAny<Expression<Func<UserRole, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync(mockedResultObject);

            // Act
            var result = await queryHandler.HandleAsync(new Query.Domain.Queries.GetRolesQuery());

            // Assert
            Assert.Equal(mockedResultObject, result.executionData);
        }
    }
}

