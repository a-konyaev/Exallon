using System;
using Enterra.V8x1C.DOM;

namespace Exallon.Sessions
{
    /// <summary>
    /// Интерфейс менеджера сессий
    /// </summary>
    public interface ISessionManager : ISubsystem
    {
        /// <summary>
        /// Создать (открыть) новую сессию
        /// </summary>
        /// <param name="login">имя пользователя</param>
        /// <param name="password">пароль пользователя</param>
        /// <returns>идентификатор созданной сессии</returns>
        string CreateNewSession(string login, string password);

        /// <summary>
        /// Закрыть сессию
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        void CloseSession(string sessionId);

        /// <summary>
        /// Получить сессию по идентификатору
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        Session GetSession(string sessionId);

        /// <summary>
        /// Выполнить операцию в контексте сессии
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="func"></param>
        T ExecuteWithinSession<T>(string sessionId, Func<Session, T> func);
    }
}
