using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Exallon.Configurations;

namespace Exallon.ConfigurationManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseConfigurationService();
        }

        /// <summary>
        /// Установить строку состояния
        /// </summary>
        /// <param name="message">текст сообщения</param>
        /// <param name="isError">имеет ли сообщение статус Ошибки</param>
        private void SetStatus(string message, bool isError)
        {
            _textStatus.Text = string.Format("{0:HH:mm:ss}: {1}", DateTime.Now, message);
            _textStatus.Foreground = isError ? Brushes.Red : Brushes.Blue;
        }

        /// <summary>
        /// Очистить строку состояния
        /// </summary>
        private void ClearStatus()
        {
            _textStatus.Text = "";
        }

        /// <summary>
        /// Обработка изменения активной закладки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearStatus();
        }

        /// <summary>
        /// Обработка нажатия кнопки "Подключиться"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        /// <summary>
        /// Обработка нажатия кнопки "Применить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            CallConfigurationService(
                "Обновление конфигурации",
                () =>
                    {
                        if (_tabCatalogReqs.IsSelected)
                        {
                            _configurationService.UpdateCatalogsView((List<CatalogView>) _listViewCatalogs.ItemsSource);
                        }
                        else
                        {
                            _configurationService.UpdateDocumentsView((List<DocumentView>)_listViewDocuments.ItemsSource);
                        }
                    });
        }

        /// <summary>
        /// Обработка нажатия кнопки "Отмена"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Взаимодействие с сервисом конфигурации

        private ChannelFactory<IConfigurationService> _proxy;
        private IConfigurationService _configurationService;

        private void Connect()
        {
            CallConfigurationService(
                "Подключение к серверу",
                () =>
                    {
                        OpenConfigurationService();

                        var catsView = _configurationService.GetCatalogsView();
                        if (catsView.Count > 0)
                        {
                            _listViewCatalogs.ItemsSource = catsView;
                        }

                        var docsView = _configurationService.GetDocumentsView();
                        if (docsView.Count > 0)
                        {
                            _listViewDocuments.ItemsSource = docsView;
                        }

                        if (catsView.Count == 0 || docsView.Count == 0)
                        {
                            var loginWin = new LoginWindow();
                            if (!(loginWin.ShowDialog() ?? false))
                                throw new Exception("Инициализация конфигурации не выполнена");

                            if (catsView.Count == 0)
                            {
                                _configurationService.InitCatalogsView(loginWin.Login, loginWin.Password);
                                _listViewCatalogs.ItemsSource = _configurationService.GetCatalogsView();        
                            }

                            if (docsView.Count == 0)
                            {
                                _configurationService.InitDocumentsView(loginWin.Login, loginWin.Password);
                                _listViewDocuments.ItemsSource = _configurationService.GetDocumentsView();
                            }
                        }
                    });
        }

        private void OpenConfigurationService()
        {
            try
            {
                _proxy = new ChannelFactory<IConfigurationService>("ExallonConfig");
                _proxy.Open();
                _buttonConnect.IsEnabled = false;
            }
            catch
            {
                _proxy = null;
                throw;
            }

            _configurationService = _proxy.CreateChannel();
        }

        private void CloseConfigurationService()
        {
            if (_proxy == null || _proxy.State != CommunicationState.Opened)
                return;

            try
            {
                _proxy.Close(TimeSpan.FromSeconds(1));
            }
            finally
            {
                _proxy = null;
                _configurationService = null;
                _buttonApply.IsEnabled = false;
                _buttonConnect.IsEnabled = true;
            }
        }

        /// <summary>
        /// Выполнить операцию, вызвав серсис конфигурации
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        private void CallConfigurationService(string actionName, Action action)
        {
            Cursor = Cursors.Wait;
            SetStatus(actionName + "...", false);

            Dispatcher.BeginInvoke(
                new Action(
                    delegate
                        {
                            try
                            {
                                action();
                                SetStatus(actionName + " выполнено успешно", false);
                            }
                            catch
                            {
                                SetStatus(actionName + " завершилось ошибкой", true);
                                CloseConfigurationService();
                            }
                            finally
                            {
                                Cursor = Cursors.Arrow;
                            }
                        }));
        }

        #endregion

        #region Справочники

        /// <summary>
        /// Флаг "Отображать" в заголовке списка "Реквизиты справочника"
        /// </summary>
        private CheckBox _checkBoxCatalogRequisitesHeaderShow;

        /// <summary>
        /// Обработка клика на флаг в заголовке "Отображать" списка "Реквизиты справочника"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CatalogRequisitesHeaderShow_CheckChanged(object sender, RoutedEventArgs e)
        {
            _checkBoxCatalogRequisitesHeaderShow = (CheckBox)sender;
            var show = ((CheckBox)e.OriginalSource).IsChecked ?? false;
            foreach(RequisiteView item in _listViewCatalogRequisites.ItemsSource)
            {
                item.Show = show && ReqTypeToEnabledConverter.ReqTypeToEnabled(item.TypeInfo.Type);
            }

            CollectionViewSource.GetDefaultView(_listViewCatalogRequisites.ItemsSource).Refresh();
        }

        /// <summary>
        /// Обработка клика на флаг в заголовке "Отображать" списка "Справочники"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CatalogsHeaderShow_CheckChanged(object sender, RoutedEventArgs e)
        {
            var show = ((CheckBox)e.OriginalSource).IsChecked ?? false;
            foreach (CatalogView item in _listViewCatalogs.ItemsSource)
            {
                item.Show = show;
            }

            CollectionViewSource.GetDefaultView(_listViewCatalogs.ItemsSource).Refresh();
        }

        /// <summary>
        /// Обработка выбора элемента списка "Справочники"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewCatalogs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_checkBoxCatalogRequisitesHeaderShow == null)
                return;

            _checkBoxCatalogRequisitesHeaderShow.IsChecked = false;
        }

        #endregion

        #region Документы

        /// <summary>
        /// Флаг "Отображать" в заголовке списка "Реквизиты документа"
        /// </summary>
        private CheckBox _checkBoxDocumentRequisitesHeaderShow;

        /// <summary>
        /// Обработка клика на флаг в заголовке "Отображать" списка "Реквизиты документа"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentRequisitesHeaderShow_CheckChanged(object sender, RoutedEventArgs e)
        {
            _checkBoxDocumentRequisitesHeaderShow = (CheckBox)sender;
            var show = ((CheckBox)e.OriginalSource).IsChecked ?? false;
            foreach (RequisiteView item in _listViewDocumentRequisites.ItemsSource)
            {
                item.Show = show && ReqTypeToEnabledConverter.ReqTypeToEnabled(item.TypeInfo.Type);
            }

            CollectionViewSource.GetDefaultView(_listViewDocumentRequisites.ItemsSource).Refresh();
        }

        /// <summary>
        /// Обработка клика на флаг в заголовке "Отображать" списка "Документы"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentsHeaderShow_CheckChanged(object sender, RoutedEventArgs e)
        {
            var show = ((CheckBox)e.OriginalSource).IsChecked ?? false;
            foreach (DocumentView item in _listViewDocuments.ItemsSource)
            {
                item.Show = show;
            }

            CollectionViewSource.GetDefaultView(_listViewDocuments.ItemsSource).Refresh();
        }

        /// <summary>
        /// Обработка выбора элемента списка "Документы"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_checkBoxDocumentRequisitesHeaderShow == null)
                return;

            _checkBoxDocumentRequisitesHeaderShow.IsChecked = false;
        }

        #endregion
    }
}
