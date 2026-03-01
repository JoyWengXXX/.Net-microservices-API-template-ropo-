using UserRoleService.Cmd.Domain.Events;
using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using RoleService.Cmd.Domain.Handlers;
using Service.Common.Middleware;

namespace RoleService.Cmd.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repo;

        public EventHandler(IRepository<MainDBConnectionManager> repo)
        {
            _repo = repo;
        }

        public async Task<TResult> On(AddRoleEvent @event)
        {
            await _repo.CreateAsync(new UserRole
            {
                RoleId = @event.id,
                RoleOrder = @event.roleOrder,
                RoleName = @event.roleName,
                IsEnable = true,
                CreateDate = DateTime.UtcNow,
            });

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(UpdateRoleEvent @event)
        {
            using var mainUOW = _repo.CreateUnitOfWork();
            var role = await _repo.GetFirstAsync<UserRole>(x => new { x.RoleId }, x => x.RoleId == @event.roleId, unitOfWork: mainUOW);
            if (role == null)
            {
                throw new AppException("Not found role!");
            }

            mainUOW.Begin();
            try
            {
                await _repo.UpdateAsync<UserRole>(x => x.RoleOrder == @event.roleOrder && x.RoleName == @event.roleName, x => x.RoleId == role.RoleId, mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(DisableRoleEvent @event)
        {
            using var mainUOW = _repo.CreateUnitOfWork();
            var role = await _repo.GetFirstAsync<UserRole>(x => new { x.RoleId }, x => x.RoleId == @event.roleId, unitOfWork: mainUOW);
            if (role == null)
            {
                throw new AppException("Not found role!");
            }

            mainUOW.Begin();
            try
            {
                await _repo.UpdateAsync<UserRole>(x => x.IsEnable == false, x => x.RoleId == role.RoleId, mainUOW);
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

