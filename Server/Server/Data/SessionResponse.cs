using System.Runtime.Serialization;

namespace Exallon.Data
{
    /// <summary>
    /// Ответ с информацией о сессии
    /// </summary>
    [DataContract]
    public class SessionResponse : Response
    {
        /// <summary>
        /// Идентификатор сессии
        /// </summary>
        [DataMember]
        public string SessionId { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        public SessionResponse(string sessionId)
        {
            SessionId = sessionId;
        }

        /// <summary>
        /// Конструктор с указанием кода ошибки
        /// </summary>
        /// <param name="errorCode"></param>
        public SessionResponse(int errorCode)
            : base(errorCode)
        {
        }

        /// <summary>
        /// Конструктор с указанием кода и описания ошибки
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorDescription"></param>
        public SessionResponse(int errorCode, string errorDescription)
            : base(errorCode, errorDescription)
        {
        }
    }
}
