using System.ServiceModel;

namespace Exallon.Data
{
    /// <summary>
    /// Интерфейс сервиса работы с данными
    /// </summary>
    [ServiceContract(Namespace = "http://www.exallon.ru", Name = "DataService")]
    public interface IDataService
    {
        /// <summary>
        /// Авторизация в системе
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        /// <returns>результат выполнения операции</returns>
        [OperationContract]
        SessionResponse Authorize(string login, string password);

        /// <summary>
        /// Закрытие сессии
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        [OperationContract]
        SessionResponse Close(string sessionId);
        
        /// <summary>
        /// Получение списка справочников
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        [OperationContract]
        GetDataResponse GetCatalogs(string sessionId);
        
        /// <summary>
        /// Получение элементов справочника
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <param name="catalogName">имя справочника</param>
        /// <param name="indexFrom">индекс элемента, начиная с которого нужно вернуть данные</param>
        /// <param name="indexTo">индекс элемента, заканчивая которым нужно вернуть данные</param>
        /// <param name="parentId">идентификатор родительского элемента (для иерархических справочников)</param>
        /// <returns></returns>
        /// <remarks>
        /// Если indexFrom и/или indexTo заданы, то возвращаются элементы справочника,
        /// индекс которых попадают в отрезок [indexFrom; indexTo].
        /// Если indexFrom и indexTo не заданы, то возвращаются все элементы справочника.
        /// Нумерация элементов начинается с 0.
        /// </remarks>
        [OperationContract]
        GetDataResponse GetCatalogItems(
            string sessionId, 
            string catalogName, 
            int indexFrom,
            int indexTo,
            string parentId);

        /// <summary>
        /// Получить все элементы справочника
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="catalogName"></param>
        /// <returns></returns>
        [OperationContract]
        GetDataResponse GetAllCatalogItems(string sessionId, string catalogName);
        
        /// <summary>
        /// Получить детальную информацию по элементу справочника
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <param name="catalogName">имя справочника</param>
        /// <param name="itemId">идентификатор элемента справочника</param>
        /// <returns></returns>
        [OperationContract]
        GetDataResponse GetCatalogItemDetails(string sessionId, string catalogName, string itemId);

        /// <summary>
        /// Получение списка документов
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        [OperationContract]
        GetDataResponse GetDocuments(string sessionId);
    }
}
