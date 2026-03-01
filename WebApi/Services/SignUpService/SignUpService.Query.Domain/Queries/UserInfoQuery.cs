using CQRS.Core.Queries;

namespace SignUpService.Query.Domain.Queries
{
    public class UserInfoQuery : BaseQuery
    {
        public string userId { get; set; }
    }
}

