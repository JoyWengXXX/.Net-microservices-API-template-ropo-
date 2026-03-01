namespace Service.Background.Services
{
    public interface IPostgresChangeDetector
    {
        event EventHandler<string> OnDataChanged;

        void Dispose();
        Task StartListening(string tableName);
        void StopListening();
    }
}
