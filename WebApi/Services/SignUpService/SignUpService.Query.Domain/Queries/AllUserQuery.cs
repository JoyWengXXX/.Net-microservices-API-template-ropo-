using CQRS.Core.Queries;
using SignUpService.Query.Domain.DTOs;

namespace SignUpService.Query.Domain.Queries
{
    public class AllUserQuery : BaseQuery
    {
        public QueryAllUserDTO input { get; set; }
    }
}

