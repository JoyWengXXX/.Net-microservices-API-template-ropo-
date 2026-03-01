using UserRolePermissionService.Cmd.Domain.Events;
using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using RolePermissionService.Cmd.Domain.Handlers;
using Service.Common.Middleware;

namespace RolePermissionService.Cmd.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repo;

        public EventHandler(IRepository<MainDBConnectionManager> repo)
        {
            _repo = repo;
        }

        public async Task<TResult> On(UpdateRolePermissionEvent @event)
        {
            var mainUOW = _repo.CreateUnitOfWork();
            mainUOW.Begin();
            try
            {
                await _repo.DeleteAsync<SystemMain.Entities.UserRoleBinding>(x => x.UserId == @event.userId, mainUOW);
                foreach (var permission in @event.permissions)
                {
                    var userPermissions = await _repo.GetFirstAsync<Controller>(x => new { x.ControllerId }, x => x.ControllerId == permission.ControllerId, null, mainUOW);
                    if (userPermissions == null)
                    {
                        throw new AppException("Not found controller!");
                    }
                    await _repo.CreateAsync(new SystemMain.Entities.UserRoleBinding() 
                            {
                                UserId = @event.userId,
                                RoleId = @event.roleId,
                                ControllerId = userPermissions.ControllerId,
                                CreateAllowed = permission.CreateAllowed,
                                QueryAllowed = permission.QueryAllowed,
                                UpdateAllowed = permission.UpdateAllowed,
                                DeleteAllowed = permission.DeleteAllowed,
                                CreateDate = DateTime.UtcNow
                            }, mainUOW);
                
                }
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }
    }
}


