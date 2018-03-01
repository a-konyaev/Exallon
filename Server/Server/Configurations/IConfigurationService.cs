using System.Collections.Generic;
using System.ServiceModel;

namespace Exallon.Configurations
{
    /// <summary>
    /// Интерфейс сервиса администрирования
    /// </summary>
    [ServiceContract(Namespace = "http://www.exallon.ru", Name = "ConfigurationService")]
    public interface IConfigurationService
    {
        #region Справочники

        /// <summary>
        /// Инициализация описания отображения справочников
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        [OperationContract]
        void InitCatalogsView(string login, string password);

        /// <summary>
        /// Получить описания отображения справочников
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CatalogView> GetCatalogsView();

        /// <summary>
        /// Обновить описания отображения справочников
        /// </summary>
        /// <param name="catalogsView"></param>
        [OperationContract]
        void UpdateCatalogsView(List<CatalogView> catalogsView);

        #endregion

        #region Документы

        /// <summary>
        /// Инициализация описания отображения документов
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        [OperationContract]
        void InitDocumentsView(string login, string password);

        /// <summary>
        /// Получить описания отображения документов
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<DocumentView> GetDocumentsView();

        /// <summary>
        /// Обновить описания отображения документов
        /// </summary>
        /// <param name="documentsView"></param>
        [OperationContract]
        void UpdateDocumentsView(List<DocumentView> documentsView);

        #endregion
    }
}
