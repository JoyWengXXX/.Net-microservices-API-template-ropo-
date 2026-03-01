using System.Linq.Expressions;
using ControllerService.Cmd.Domain.Events;

namespace ControllerService.Tests
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
        public async Task On_AddControllerEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var @event = new AddControllerEvent
            {
                controllerName = "testController",
            };
            mockRepo.Setup(x => x.CreateAsync(It.IsAny<Controller>(), It.IsAny<IUnitOfWork>())).ReturnsAsync(1);

            // Act
            var result = await _eventHandler.On(@event);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_UpdateControllerEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var @event = new UpdateControllerEvent
            {
                controllerId = "TEST_Controller",
                controllerName = "testController",
            };
            Controller mockedControllerObject = new Controller
            {
                ControllerId = @event.controllerId,
                ControllerName = "testController",
            };
            mockRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<Controller, object>>>(), It.IsAny<Expression<Func<Controller, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync(mockedControllerObject);
            mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Expression<Func<Controller, bool>>>(), It.IsAny<Expression<Func<Controller, bool>>>(), It.IsAny<IUnitOfWork>())).Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(@event);

            // Assert
            Assert.True(result.isSuccess);
        }

        [Fact]
        public async Task On_DisableControllerEvent_ReturnsTResultWithSuccess()
        {
            // Arrange
            var @event = new DisableControllerEvent
            {
                controllerId = "TEST_Controller",
            };
            Controller mockedControllerObject = new Controller
            {
                ControllerId = @event.controllerId,
            };
            mockRepo.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<Controller, object>>>(), It.IsAny<Expression<Func<Controller, bool>>>(), null, It.IsAny<IUnitOfWork>())).ReturnsAsync(mockedControllerObject);
            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<Expression<Func<Controller, bool>>>(), It.IsAny<IUnitOfWork>())).Returns(Task.FromResult(1));

            // Act
            var result = await _eventHandler.On(@event);

            // Assert
            Assert.True(result.isSuccess);
        }
    }
}

