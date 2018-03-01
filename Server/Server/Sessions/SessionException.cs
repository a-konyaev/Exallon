using System;

namespace Exallon.Sessions
{
    /// <summary>
    /// Исключение при работе с сессией
    /// </summary>
    public class SessionException : Exception
    {
        /// <summary>
        /// Идентификатор сессии
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="sessionId"></param>
        public SessionException(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
