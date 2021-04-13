using Network;

namespace Matchmaker.Services
{
    /// <summary>
    /// Интерфейс простого матчмейкера.
    /// </summary>
    public interface IMatchmakerService
    {
        /// <summary>
        /// Метод позволяет встать в очередь на участие в матче.
        /// </summary>
        /// <param name="userId">идентификатор пользователя.</param>
        /// <returns>true, если пользователь был добавлен в очередь, false в противном случае.</returns>
        bool Enqueue(string userId);
        /// <summary>
        /// Метод позволяет узнать статус пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns><see cref="UserStatus"/></returns>
        UserStatus GetStatus(string userId);
        /// <summary>
        /// Метод позволяет получить порт матча, в котором учавствует пользователь.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Порт матча.</returns>
        int? GetMatch(string userId);
        /// <summary>
        /// Метод позволяет пользователю покинуть очередь на участие в матче.
        /// </summary>
        /// <param name="userId">id пользователя.</param>
        /// <returns>true, если пользователю удалось покинуть очередь, false в противном случае.</returns>
        bool Remove(string userId);
    }
}
