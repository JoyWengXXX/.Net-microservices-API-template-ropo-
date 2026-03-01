
namespace Service.Background.Services
{
    public class LogInTokenClearService : ILogInTokenClearService
    {
        private readonly IRepository<MainDBConnectionManager> _repo;

        public LogInTokenClearService(IRepository<MainDBConnectionManager> repo)
        {
            _repo = repo;
        }

        public async Task ClearAllDisabledLoginToken()
        {
            await _repo.DeleteAsync<LogInRecord>(x => x.IsEnable == false);
        }
    }
}

