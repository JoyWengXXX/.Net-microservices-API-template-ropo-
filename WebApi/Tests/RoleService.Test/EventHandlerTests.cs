using System.Linq.Expressions;
using UserRoleService.Cmd.Domain.Events;

namespace RoleService.Tests
{
    public class EventHandlerTests
    {
        private readonly Mock<IRepository<MainDBConnectionManager>> mockRepo;
        private readonly Cmd.Infrastructure.Handlers.EventHandler _eventHandler;

        public EventHandlerTests()
        {
            mockRepo = new Mock<IRepository<MainDBConnectionManager>>();
            _eventHandler = new Cmd.Infrastructure.Handlers.EventHandler(mockRepo.Object);
        }

        [Fact]
        public async Task On_AddRoleEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var @event = new AddRoleEvent
            {
                roleName = "testRole",
            };
            mockRepo.Setup(x => x.CreateAsync(It.IsAny<UserRole>(), It.IsAny<IUnitOfWork>())).ReturnsAsync(1);

            // Act
            var result = await _eventHandler.On(@event);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_UpdateRoleEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var @event = new UpdateRoleEvent
            {
                roleId = Guid.NewGuid(),
                roleName = "testRole",
            };
            UserRole mockedRoleObject = new UserRole
            {
                RoleId = @event.roleId,
                RoleName = "testRole",
            };
            mockRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserRole, object>>>(), It.IsAny<Expression<Func<UserRole, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync(mockedRoleObject);
            mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<IUnitOfWork>())).Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(@event);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_DisableRoleEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var @event = new DisableRoleEvent
            {
                roleId = Guid.NewGuid(),
            };
            UserRole mockedRoleObject = new UserRole
            {
                RoleId = @event.roleId,
            };
            mockRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<UserRole, object>>>(), It.IsAny<Expression<Func<UserRole, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync(mockedRoleObject);
            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<IUnitOfWork>())).Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(@event);

            // Assert
            Assert.True(result.isSuccess);
        }
    }
}

