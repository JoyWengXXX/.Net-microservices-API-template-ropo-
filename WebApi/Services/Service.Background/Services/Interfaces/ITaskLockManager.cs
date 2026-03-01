namespace Service.Background.Services.Interfaces
{
    /// <summary>
    /// ä»»å??–å?ç®¡ç??¨ä??¢ï??¨æ–¼ç¢ºä?ä»»å?ä¸æ??è??·è?
    /// </summary>
    public interface ITaskLockManager
    {
        /// <summary>
        /// ?—è©¦?²å?ä»»å??–å?
        /// </summary>
        /// <param name="taskName">ä»»å??ç¨±</param>
        /// <returns>å¦‚æ??å??²å??–å?è¿”å?trueï¼Œå¦?‡è??false</returns>
        bool TryAcquireLock(string taskName);

        /// <summary>
        /// ?‹æ”¾ä»»å??–å?
        /// </summary>
        /// <param name="taskName">ä»»å??ç¨±</param>
        void ReleaseLock(string taskName);

        /// <summary>
        /// æª¢æŸ¥ä»»å??¯å¦æ­?œ¨?·è?
        /// </summary>
        /// <param name="taskName">ä»»å??ç¨±</param>
        /// <returns>å¦‚æ?ä»»å?æ­?œ¨?·è?è¿”å?trueï¼Œå¦?‡è??false</returns>
        bool IsTaskRunning(string taskName);
    }
}

