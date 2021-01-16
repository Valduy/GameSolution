namespace MatchmakerModels
{
    /// <summary>
    /// Возможные статусы ползьзователя.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>В очереди.</summary>
        Waited,
        /// <summary>Присоединен к матчу.</summary>
        Connected,
        /// <summary>Отсутствует.</summary>
        Absented,
    }
}
