namespace Service.Background.Services.Interfaces
{
    public interface ILogInTokenClearService
    {
        Task ClearAllDisabledLoginToken();
    }
}
