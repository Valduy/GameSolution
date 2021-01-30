namespace MatchmakerModels
{
    /// <summary>
    /// Возможные статусы ползьзователя.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>В очереди.</summary>
        Wait,
        /// <summary>Присоединен к матчу.</summary>
        Connected,
        /// <summary>Отсутствует.</summary>
        Absent,
    }
}
