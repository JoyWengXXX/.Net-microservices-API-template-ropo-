using System.Linq.Expressions;
using ControllerService.Query.Domain.Queries;
using ControllerService.Query.Infrastructure.Handlers;

namespace ControllerService.Tests;

public class QueryHandlerTests
{
    private readonly Mock<IRepository<MainDBConnectionManager>> _mockRepo;
    private readonly QueryHandler _sut;

    public QueryHandlerTests()
    {
        _mockRepo = new Mock<IRepository<MainDBConnectionManager>>();
        _sut = new QueryHandler(_mockRepo.Object);
    }

    // ── HandleAsync(GetControllersQuery) ──────────────────────────────────

    [Fact]
    public async Task HandleAsync_GetControllersQuery_WithData_ReturnsAllControllers()
    {
        // Arrange
        var mockList = new List<Controller>
        {
            new Controller { ControllerId = "CTRL_001", ControllerName = "Controller A" },
            new Controller { ControllerId = "CTRL_002", ControllerName = "Controller B" },
        };
        _mockRepo.Setup(x => x.GetListAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            null))
            .ReturnsAsync(mockList);

        // Act
        var result = await _sut.HandleAsync(new GetControllersQuery());

        // Assert
        Assert.True(result.isSuccess);
        var data = Assert.IsType<List<Controller>>(result.executionData);
        Assert.Equal(2, data.Count);
        _mockRepo.Verify(x => x.GetListAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetControllersQuery_WhenEmpty_ThrowsAppException()
    {
        // Arrange
        _mockRepo.Setup(x => x.GetListAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            null))
            .ReturnsAsync(new List<Controller>());

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() =>
            _sut.HandleAsync(new GetControllersQuery()));
    }

    [Fact]
    public async Task HandleAsync_GetControllersQuery_WhenNull_ThrowsAppException()
    {
        // Arrange
        _mockRepo.Setup(x => x.GetListAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            null))
            .ReturnsAsync((IEnumerable<Controller>)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() =>
            _sut.HandleAsync(new GetControllersQuery()));
    }

    // ── HandleAsync(GetControllerByIdQuery) ───────────────────────────────

    [Fact]
    public async Task HandleAsync_GetControllerByIdQuery_WhenFound_ReturnsController()
    {
        // Arrange
        var existing = new Controller { ControllerId = "CTRL_001", ControllerName = "My Controller" };
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            null))
            .ReturnsAsync(existing);

        // Act
        var result = await _sut.HandleAsync(new GetControllerByIdQuery { ControllerId = "CTRL_001" });

        // Assert
        Assert.True(result.isSuccess);
        var data = Assert.IsType<Controller>(result.executionData);
        Assert.Equal("CTRL_001", data.ControllerId);
        _mockRepo.Verify(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetControllerByIdQuery_WhenNotFound_ThrowsAppException()
    {
        // Arrange
        _mockRepo.Setup(x => x.GetFirstAsync(
            It.IsAny<Expression<Func<Controller, object>>>(),
            It.IsAny<Expression<Func<Controller, bool>>>(),
            null,
            null))
            .ReturnsAsync((Controller)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() =>
            _sut.HandleAsync(new GetControllerByIdQuery { ControllerId = "NON_EXISTENT" }));
    }
}
