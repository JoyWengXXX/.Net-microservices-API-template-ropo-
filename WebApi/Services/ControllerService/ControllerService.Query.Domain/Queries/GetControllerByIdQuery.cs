using CQRS.Core.Queries;

namespace ControllerService.Query.Domain.Queries
{
    public class GetControllerByIdQuery : BaseQuery
    {
        public string ControllerId { get; set; } = null!;
    }
}
