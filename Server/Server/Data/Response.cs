using System.Runtime.Serialization;

namespace Exallon.Data
{
    /// <summary>
    /// Базовый класс для ответов, возвращаемых в результате обработки запроса на выполнение некоторого действия
    /// </summary>
    [DataContract]
    public abstract class Response
    {
        #region Общие коды ошибок

        /// <summary>
        /// Код ошибки "Ошибок нет"
        /// </summary>
        public const int ERROR_CODE_NO_ERROR = 0;
        /// <summary>
        /// Код ошибки "Неизвестная ошибка"
        /// </summary>
        public const int ERROR_CODE_UNKNOWN_ERROR = 1;
        /// <summary>
        /// Код ошибки "Лицензия истекла или отсутствует"
        /// </summary>
        public const int ERROR_CODE_LICENSE_EXPIRED = 2;
        /// <summary>
        /// Код ошибки "Неправильный логин и/или пароль"
        /// </summary>
        public const int ERROR_CODE_UNAUTHORIZED = 3;
        /// <summary>
        /// Код ошибки "Сессия не найдена"
        /// </summary>
        public const int ERROR_CODE_SESSION_NOT_FOUND = 4;

        #endregion

        /// <summary>
        /// Код ошибки, которая возникла при выполнении действия
        /// </summary>
        [DataMember]
        public int ErrorCode { get; private set; }
        /// <summary>
        /// Описание ошибки, которая возникла при выполнении действия
        /// </summary>
        [DataMember]
        public string ErrorDescription { get; private set; }
        /// <summary>
        /// Признак того, что при выполнении действия произошла ошибка
        /// </summary>
        public bool Failed
        {
            get { return ErrorCode != ERROR_CODE_NO_ERROR; }
        }

        protected Response()
        {
            ErrorCode = ERROR_CODE_NO_ERROR;
        }

        protected Response(int errorCode)
        {
            ErrorCode = errorCode;
        }

        protected Response(int errorCode, string errorDescription)
        {
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
        }
    }
}
