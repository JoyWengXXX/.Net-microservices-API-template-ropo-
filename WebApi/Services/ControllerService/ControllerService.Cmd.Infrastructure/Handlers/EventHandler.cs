using CQRS.Core.Infrastructure;
using DataAccess.Interfaces;
using DataAccess;
using SystemMain.Entities;
using ControllerService.Cmd.Domain.Handlers;
using Service.Common.Middleware;
using System.Linq.Expressions;
using ControllerService.Cmd.Domain.Events;

namespace ControllerService.Cmd.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repo;

        public EventHandler(IRepository<MainDBConnectionManager> repo)
        {
            _repo = repo;
        }

        public async Task<TResult> On(AddControllerEvent @event)
        {
            await _repo.CreateAsync(new Controller
            {
                ControllerId = @event.controllerId,
                ControllerName = @event.controllerName,
                IsEnable = true,
                CreateDate = DateTime.UtcNow,
            });

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(UpdateControllerEvent @event)
        {
            using var mainUOW = _repo.CreateUnitOfWork();
            Expression<Func<Controller, object>> select = x => new { x.ControllerId };
            Expression<Func<Controller, bool>> where1 = x => x.ControllerId == @event.controllerId;
            var controller = await _repo.GetFirstAsync(select, where1, unitOfWork: mainUOW);
            if (controller == null)
            {
                throw new AppException("Not found controller!");
            }
            mainUOW.Begin();
            try
            {
                Expression<Func<Controller, bool>> input = x => x.ControllerName == @event.controllerName;
                Expression<Func<Controller, bool>> where2 = x => x.ControllerId == controller.ControllerId;
                await _repo.UpdateAsync(input, where2, mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult { isSuccess = true };
        }

        public async Task<TResult> On(DisableControllerEvent @event)
        {
            using var mainUOW = _repo.CreateUnitOfWork();
            Expression<Func<Controller, object>> select = x => new { x.ControllerId };
            Expression<Func<Controller, bool>> where1 = x => x.ControllerId == @event.controllerId;
            var controller = await _repo.GetFirstAsync(select, where1, unitOfWork: mainUOW);
            if (controller == null)
            {
                throw new AppException("Not found controller!");
            }
            mainUOW.Begin();
            try
            {
                Expression<Func<Controller, bool>> input = x => x.IsEnable == false;
                Expression<Func<Controller, bool>> where2 = x => x.ControllerId == controller.ControllerId;
                await _repo.UpdateAsync(input, where2, mainUOW);
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

