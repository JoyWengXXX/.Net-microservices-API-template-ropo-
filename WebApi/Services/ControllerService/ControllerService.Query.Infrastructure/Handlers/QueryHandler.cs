using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using ControllerService.Query.Domain.Queries;
using ControllerService.Query.Domain.Queries.Interfaces;
using Service.Common.Middleware;
using System.Linq.Expressions;

namespace ControllerService.Query.Infrastructure.Handlers
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IRepository<MainDBConnectionManager> repo;

        public QueryHandler(IRepository<MainDBConnectionManager> pageRepo)
        {
            repo = pageRepo;
        }

        public async Task<TResult> HandleAsync(GetControllersQuery query)
        {
            Expression<Func<Controller, object>> select = x => new { x.ControllerId, x.ControllerName, x.IsEnable };
            Expression<Func<Controller, bool>> where = x => 1 == 1;
            var pages = await repo.GetListAsync(select, where);
            if (!pages.Any())
            {
                throw new AppException("Not result found");
            }
            return new TResult { isSuccess = true, executionData = pages.ToList() };
        }
    }
}

