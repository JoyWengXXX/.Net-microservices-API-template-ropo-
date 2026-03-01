
namespace ControllerService.Query.Domain.DTOs
{
    public class GetControllerDTO
    {
        public Guid controllerId { get; set; }
        public string controllerName { get; set; } = null!;
    }
}

