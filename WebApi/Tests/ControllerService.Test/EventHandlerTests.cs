using System.Linq.Expressions;
using ControllerService.Cmd.Domain.Events;
using InfraEventHandler = ControllerService.Cmd.Infrastructure.Handlers.EventHandler;

namespace ControllerService.Tests;

public class EventHandlerTests
{
    private readonly Mock<IRepository<MainDBConnectionManager>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUoW;
    private readonly InfraEventHandler _sut;

    public EventHandlerTests()
    {
        _mockRepo = new Mock<IRepository<MainDBConnectionManager>>();
        _mockUoW = new Mock<IUnitOfWork>();
        _mockRepo.Setup(x => x.CreateUnitOfWork()).Returns(_mockUoW.Object);
        _sut = new InfraEventHandler(_mockRepo.Object);
    }

    // ── On(AddControllerEvent) ────────────────────────────────────────────

    [Fact]
    public async Task On_AddControllerEvent_ReturnsTResultWithSuccess()
    {
        // Arrange
        var @event = new AddControllerEvent
        {
            controllerId = "CTRL_001",
            controllerName = "TestController"
        };
        _mockRepo.Setup(x => x.CreateAsync(
            It.IsAny<Controller>(),
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.On(@event);

        // Assert
        Assert.True(result.isSuccess);
        _mockRepo.Verify(x => x.CreateAsync(
            It.Is<Controller>(c =>
                c.ControllerId == "CTRL_001" &&
                c.ControllerName == "TestController" &&
                c.IsEnable == true),
            It.IsAny<IUnitOfWork>()), Times.Once);
    }

    // ── On(UpdateControllerEvent) ─────────────────────────────────────────

    [Fact]
    public async Task On_UpdateControllerEvent_WhenControllerExists_ReturnsTResultWithSuccess()
    {
        // Arrange
        var @event = new UpdateControllerEvent
        {
            controllerId = "CTRL_001",
            controllerName = "UpdatedName"
        };
        var existing = new Controller { ControllerId = "CTRL_001" };
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.On(@event);

        // Assert
        Assert.True(result.isSuccess);
        _mockUoW.Verify(x => x.Begin(), Times.Once);
        _mockUoW.Verify(x => x.Commit(), Times.Once);
        _mockUoW.Verify(x => x.Rollback(), Times.Never);
        _mockRepo.Verify(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()), Times.Once);
    }

    [Fact]
    public async Task On_UpdateControllerEvent_WhenControllerNotFound_ThrowsAppException()
    {
        // Arrange
        var @event = new UpdateControllerEvent
        {
            controllerId = "NON_EXISTENT",
            controllerName = "X"
        };
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync((Controller)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _sut.On(@event));
        _mockUoW.Verify(x => x.Rollback(), Times.Once);
        _mockUoW.Verify(x => x.Commit(), Times.Never);
        _mockRepo.Verify(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()), Times.Never);
    }

    [Fact]
    public async Task On_UpdateControllerEvent_WhenUpdateFails_CallsRollbackAndRethrows()
    {
        // Arrange
        var @event = new UpdateControllerEvent { controllerId = "CTRL_001", controllerName = "X" };
        var existing = new Controller { ControllerId = "CTRL_001" };
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.On(@event));
        _mockUoW.Verify(x => x.Rollback(), Times.Once);
        _mockUoW.Verify(x => x.Commit(), Times.Never);
    }

    // ── On(DisableControllerEvent) ────────────────────────────────────────

    [Fact]
    public async Task On_DisableControllerEvent_WhenControllerExists_ReturnsTResultWithSuccess()
    {
        // Arrange
        var @event = new DisableControllerEvent { controllerId = "CTRL_001" };
        var existing = new Controller { ControllerId = "CTRL_001" };
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.On(@event);

        // Assert
        Assert.True(result.isSuccess);
        _mockUoW.Verify(x => x.Begin(), Times.Once);
        _mockUoW.Verify(x => x.Commit(), Times.Once);
        _mockUoW.Verify(x => x.Rollback(), Times.Never);
        _mockRepo.Verify(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()), Times.Once);
    }

    [Fact]
    public async Task On_DisableControllerEvent_WhenControllerNotFound_ThrowsAppException()
    {
        // Arrange
        var @event = new DisableControllerEvent { controllerId = "NON_EXISTENT" };
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync((Controller)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _sut.On(@event));
        _mockUoW.Verify(x => x.Rollback(), Times.Once);
        _mockUoW.Verify(x => x.Commit(), Times.Never);
        _mockRepo.Verify(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()), Times.Never);
    }

    [Fact]
    public async Task On_DisableControllerEvent_WhenUpdateFails_CallsRollbackAndRethrows()
    {
        // Arrange
        var @event = new DisableControllerEvent { controllerId = "CTRL_001" };
        var existing = new Controller { ControllerId = "CTRL_001" };
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            It.IsAny<IUnitOfWork>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.UpdateAsync(
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            It.IsAny<IUnitOfWork>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.On(@event));
        _mockUoW.Verify(x => x.Rollback(), Times.Once);
        _mockUoW.Verify(x => x.Commit(), Times.Never);
    }
}
