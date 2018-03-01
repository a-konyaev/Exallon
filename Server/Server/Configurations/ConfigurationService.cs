using System.Collections.Generic;
using System.ServiceModel;
using Exallon.Utils;

namespace Exallon.Configurations
{
    /// <summary>
    /// Сервис администрирования
    /// </summary>
    [ServiceBehavior(Namespace = "http://www.exallon.ru", InstanceContextMode = InstanceContextMode.Single)]
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly NLog.Logger _logger;

        public ConfigurationService()
        {
            // получаем ссылки на нужные компоненты системы
            _configurationManager = Application.Instance.GetSubsystem<IConfigurationManager>();
            _logger = _configurationManager.Logger;
        }

        #region IConfigurationService Members

        #region Справочники

        /// <summary>
        /// Инициализация описания отображения справочников
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        public void InitCatalogsView(string login, string password)
        {
            _logger.Info("{0}->InitCatalogsView call...", NetHelper.GetClientAddress());
            _configurationManager.InitCatalogsView(login, password);
        }

        /// <summary>
        /// Получить описания отображения справочников
        /// </summary>
        /// <returns></returns>
        public List<CatalogView> GetCatalogsView()
        {
            _logger.Info("{0}->GetCatalogsView call...", NetHelper.GetClientAddress());
            return _configurationManager.GetCatalogsView();
        }

        /// <summary>
        /// Обновить описания отображения справочников
        /// </summary>
        /// <param name="catalogsView"></param>
        public void UpdateCatalogsView(List<CatalogView> catalogsView)
        {
            _logger.Info("{0}->UpdateCatalogsView call...", NetHelper.GetClientAddress());
            _configurationManager.UpdateCatalogsView(catalogsView);
        }

        #endregion

        #region Документы

        /// <summary>
        /// Инициализация описания отображения документов
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        public void InitDocumentsView(string login, string password)
        {
            _logger.Info("{0}->InitDocumentsView call...", NetHelper.GetClientAddress());
            _configurationManager.InitDocumentsView(login, password);
        }

        /// <summary>
        /// Получить описания отображения документов
        /// </summary>
        /// <returns></returns>
        public List<DocumentView> GetDocumentsView()
        {
            _logger.Info("{0}->GetDocumentsView call...", NetHelper.GetClientAddress());
            return _configurationManager.GetDocumentsView();
        }

        /// <summary>
        /// Обновить описания отображения документов
        /// </summary>
        /// <param name="documentsView"></param>
        public void UpdateDocumentsView(List<DocumentView> documentsView)
        {
            _logger.Info("{0}->UpdateDocumentsView call...", NetHelper.GetClientAddress());
            _configurationManager.UpdateDocumentsView(documentsView);
        }

        #endregion

        #endregion
    }
}
