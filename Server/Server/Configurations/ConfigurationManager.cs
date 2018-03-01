using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Enterra.V8x1C.DOM;
using Exallon.Sessions;
using Exallon.Utils;

namespace Exallon.Configurations
{
    public class ConfigurationManager : ServiceSubsystem, IConfigurationManager
    {
        private readonly ISessionManager _sessionManager;

        /// <summary>
        /// Конструктор
        /// </summary>
        public ConfigurationManager()
            : base("ConfigurationService", typeof (ConfigurationService))
        {
            // получаем ссылки на нужные компоненты системы
            _sessionManager = Application.Instance.GetSubsystem<ISessionManager>();
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void Init()
        {
            // инициализируем путь к файлу конфигурации отображения справочников
            InitCatalogViewFilePath();
            // загружаем словарь отображения справочников
            LoadCatalogViewDict();

            // инициализируем путь к файлу конфигурации отображения документов
            InitDocumentViewFilePath();
            // загружаем словарь отображения документов
            LoadDocumentViewDict();

            // запускаем веб-сервис
            StartService();
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public override void Dispose()
        {
            StopService();
        }

        #region IConfigurationManager members

        #region Справочники

        /// <summary>
        /// Получить описания отображения справочников
        /// </summary>
        /// <returns></returns>
        public List<CatalogView> GetCatalogsView()
        {
            return _catalogViewDict == null
                       ? new List<CatalogView>(0)
                       : _catalogViewDict.Values.ToList();
        }

        /// <summary>
        /// Обновить описания отображения справочников
        /// </summary>
        /// <param name="catalogsView"></param>
        public void UpdateCatalogsView(List<CatalogView> catalogsView)
        {
            if (catalogsView == null)
                throw new ArgumentNullException("catalogsView");

            try
            {
                lock (s_catalogViewDictSync)
                {
                    // обновляем описатели отображений для каждого справочника
                    foreach (var catView in catalogsView)
                    {
                        if (!_catalogViewDict.ContainsKey(catView.Name))
                            throw new Exception(string.Format("View of catalog '{0}' not found", catView.Name));

                        _catalogViewDict[catView.Name] = catView;
                    }

                    // сохраняем измененный словарь
                    SaveCatalogViewDict(_catalogViewDict);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("UpdateCatalogsView failed", ex);
                throw;
            }
        }

        /// <summary>
        /// Получить описание отображения справочника
        /// </summary>
        /// <param name="catalogName">имя справочника</param>
        /// <returns></returns>
        public CatalogView GetCatalogView(string catalogName)
        {
            lock (s_catalogViewDictSync)
            {
                return !_catalogViewDict.ContainsKey(catalogName)
                           ? new CatalogView(string.Empty, string.Empty, false, new List<RequisiteView>(0))
                           : _catalogViewDict[catalogName];
            }
        }

        /// <summary>
        /// Инициализация описания отображения справочников
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        public void InitCatalogsView(string login, string password)
        {
            try
            {
                var sessionId = _sessionManager.CreateNewSession(login, password);

                _catalogViewDict = _sessionManager.ExecuteWithinSession(
                    sessionId,
                    session =>
                    {
                        var catalogs = session.Metadata.Catalogs;
                        var dict = new Dictionary<string, CatalogView>(catalogs.Count);

                        Logger.Trace("InitCatalogViewDict: run...");
                        // заполняем словарь
                        foreach (MetadataObject catalog in catalogs)
                        {
                            var catalogName = catalog.Name;
                            var catalogPresentation = catalog.Presentation;
                            var reqs = catalog.Requisites;
                            var reqViewList = new List<RequisiteView>(reqs.Count + 2);

                            // добавляем стандартные реквизиты
                            AddCatalogCommonRequisites(session, catalogName, reqViewList);

                            // добавляем дополнительные реквизиты
                            AddCustomRequisites(reqs, reqViewList);

                            dict.Add(catalogName, new CatalogView(catalogName, catalogPresentation, true, reqViewList));
                        }

                        // сохраняем словарь
                        SaveCatalogViewDict(dict);
                        Logger.Trace("InitCatalogViewDict: done!");

                        return dict;
                    }
                    );
            }
            catch (Exception ex)
            {
                if (File.Exists(_catalogViewFilePath))
                    File.Delete(_catalogViewFilePath);

                Logger.ErrorException("Catalog view dictionary initialization failed", ex);
                throw new Exception("Catalog view dictionary initialization failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Добавление общих реквизитов справочника
        /// </summary>
        /// <param name="session"></param>
        /// <param name="catalogName"></param>
        /// <param name="reqViewList"></param>
        private static void AddCatalogCommonRequisites(
            Session session, string catalogName, ICollection<RequisiteView> reqViewList)
        {
            var selectCode = true;
            var selectName = true;

            while (true)
            {
                try
                {
                    // сформируем строку запроса
                    var queryString = string.Format("SELECT {0}, {1} FROM Справочник.{2}",
                                                    selectCode ? "Код" : "0",
                                                    selectName ? "Наименование" : "1",
                                                    catalogName);

                    // выполняем запрос
                    using (var query = new Query(session, queryString))
                    {
                        query.Execute();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Поле не найдено \"Код\"") && selectCode)
                    {
                        selectCode = false;
                        continue;
                    }

                    if (ex.Message.Contains("Поле не найдено \"Наименование\"") && selectName)
                    {
                        selectName = false;
                        continue;
                    }

                    // TODO: надо повнимательнее проверить - стоит проверять это поле при инициализации
                    // или все таки нужно как сейчас выполнять проверку при чтении справочника
                    //if (ex.Message.Contains("Поле не найдено \"ЭтоГруппа\"") && hierarchical)
                    //{
                    //    hierarchical = false;
                    //    continue;
                    //}

                    throw new Exception(string.Format("AddCatalogCommonRequisites failed: " + ex.Message));
                }
            }

            if (selectCode)
                reqViewList.Add(
                    new RequisiteView(RequisiteView.REQUISITE_CODE_ID, "Код", "Код", true, TypeInfo.StringType));

            if (selectName)
                reqViewList.Add(
                    new RequisiteView(RequisiteView.REQUISITE_NAME_ID, "Наименование", "Наименование", true, TypeInfo.StringType));
        }

        /// <summary>
        /// Добавление настраиваемых (дополнительных) реквизитов 
        /// </summary>
        /// <param name="reqs"></param>
        /// <param name="reqViewList"></param>
        private static void AddCustomRequisites(MetadataObjectCollection reqs, ICollection<RequisiteView> reqViewList)
        {
            for (var i = 0; i < reqs.Count; i++)
            {
                BaseObject.RestDelay();

                var req = reqs[i];
                var reqName = req.Name;
                var reqPresentation = req.Presentation;

                TypeInfo reqTypeInfo;
                try
                {
                    reqTypeInfo = req.Type.Types[0];
                }
                catch (Exception ex)
                {
                    // некоторые типы реквизитов не читаются
                    if (string.CompareOrdinal(
                        ex.Message, "Значения данного типа не могут быть представлены в XML") != 0)
                    {
                        throw;
                    }

                    reqTypeInfo = new TypeInfo();
                }

                reqViewList.Add(new RequisiteView(i, reqName, reqPresentation, true, reqTypeInfo));
            }
        }

        #region Работа со словарем отображений справочников

        /// <summary>
        /// Путь к файлу сериализованного словаря отображений справочников
        /// </summary>
        private string _catalogViewFilePath;

        /// <summary>
        /// Объект синхронизации работы с словарем отображений справочников
        /// </summary>
        private static readonly object s_catalogViewDictSync = new object();

        /// <summary>
        /// Словарь отображений справочников: [имя справочника, описатель отображения справочника]
        /// </summary>
        private Dictionary<string, CatalogView> _catalogViewDict = new Dictionary<string, CatalogView>();

        /// <summary>
        /// Инициализация пути к файлу сериализованного словаря отображений справочников
        /// </summary>
        private void InitCatalogViewFilePath()
        {
            _catalogViewFilePath = Path.Combine(
                FileHelper.RootDirPath,
                Application.Instance.StubMode ? "Stub/catalogview.bin" : "catalogview.bin");
        }

        /// <summary>
        /// Сохранение словаря отображений справочников в файл
        /// </summary>
        /// <param name="catalogViewDict"></param>
        private void SaveCatalogViewDict(Dictionary<string, CatalogView> catalogViewDict)
        {
            try
            {
                using (var stream = File.Open(_catalogViewFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, catalogViewDict);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Catalog view dictionary saving failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Загрузка словаря отображений справочников
        /// </summary>
        private void LoadCatalogViewDict()
        {
            try
            {
                // если файл не найден
                if (!File.Exists(_catalogViewFilePath))
                {
                    // выходим - инициализация словаря будет выполнена при первом обращении к нему
                    Logger.Trace("LoadCatalogViewDict: catalog view file '{0}' not found", _catalogViewFilePath);
                    return;
                }

                // десериализуем словарь реквизитов
                using (var stream = File.Open(_catalogViewFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var formatter = new BinaryFormatter();
                    _catalogViewDict = (Dictionary<string, CatalogView>) formatter.Deserialize(stream);
                }

                Logger.Trace("LoadCatalogViewDict: done");
            }
            catch (Exception ex)
            {
                throw new Exception("Catalog view dictionary loading failed: " + ex.Message);
            }
        }

        #endregion

        #endregion

        #region Документы

        /// <summary>
        /// Получить описания отображения документов
        /// </summary>
        /// <returns></returns>
        public List<DocumentView> GetDocumentsView()
        {
            return _documentViewDict == null
                       ? new List<DocumentView>(0)
                       : _documentViewDict.Values.ToList();
        }

        /// <summary>
        /// Обновить описания отображения документов
        /// </summary>
        /// <param name="documentsView"></param>
        public void UpdateDocumentsView(List<DocumentView> documentsView)
        {
            if (documentsView == null)
                throw new ArgumentNullException("documentsView");

            try
            {
                lock (s_documentViewDictSync)
                {
                    // обновляем описатели отображений для каждого документа
                    foreach (var docView in documentsView)
                    {
                        if (!_documentViewDict.ContainsKey(docView.Name))
                            throw new Exception(string.Format("View of document '{0}' not found", docView.Name));

                        _documentViewDict[docView.Name] = docView;
                    }

                    // сохраняем измененный словарь
                    SaveDocumentViewDict(_documentViewDict);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("UpdateDocumentsView failed", ex);
                throw;
            }
        }

        /// <summary>
        /// Получить описание отображения документа
        /// </summary>
        /// <param name="documentName">имя документа</param>
        /// <returns></returns>
        public DocumentView GetDocumentView(string documentName)
        {
            lock (s_documentViewDictSync)
            {
                return !_documentViewDict.ContainsKey(documentName)
                           ? new DocumentView(string.Empty, string.Empty, false, new List<RequisiteView>(0))
                           : _documentViewDict[documentName];
            }
        }

        /// <summary>
        /// Инициализация описания отображения документов
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        public void InitDocumentsView(string login, string password)
        {
            try
            {
                var sessionId = _sessionManager.CreateNewSession(login, password);

                _documentViewDict = _sessionManager.ExecuteWithinSession(
                    sessionId,
                    session =>
                    {
                        var documents = session.Metadata.Documents;
                        var dict = new Dictionary<string, DocumentView>(documents.Count);

                        Logger.Trace("InitDocumentViewDict: run...");
                        // заполняем словарь
                        foreach (MetadataObject document in documents)
                        {
                            var documentName = document.Name;
                            var documentPresentation = document.Presentation;
                            var reqs = document.Requisites;
                            var reqViewList = new List<RequisiteView>(reqs.Count + 2);

                            // добавляем стандартные реквизиты
                            AddDocumentCommonRequisites(session, documentName, reqViewList);

                            // добавляем дополнительные реквизиты
                            AddCustomRequisites(reqs, reqViewList);

                            dict.Add(documentName, new DocumentView(documentName, documentPresentation, true, reqViewList));
                        }

                        // сохраняем словарь
                        SaveDocumentViewDict(dict);
                        Logger.Trace("InitDocumentViewDict: done!");

                        return dict;
                    }
                    );
            }
            catch (Exception ex)
            {
                if (File.Exists(_documentViewFilePath))
                    File.Delete(_documentViewFilePath);

                Logger.ErrorException("Document view dictionary initialization failed", ex);
                throw new Exception("Document view dictionary initialization failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Добавление общих реквизитов докмента
        /// </summary>
        /// <param name="session"></param>
        /// <param name="docunemtName"></param>
        /// <param name="reqViewList"></param>
        private static void AddDocumentCommonRequisites(
            Session session, string docunemtName, ICollection<RequisiteView> reqViewList)
        {
            var selectCode = true;
            var selectName = true;

            while (true)
            {
                try
                {
                    // сформируем строку запроса
                    var queryString = string.Format("SELECT {0}, {1} FROM Документ.{2}",
                                                    selectCode ? "Код" : "0",
                                                    selectName ? "Наименование" : "1",
                                                    docunemtName);

                    // выполняем запрос
                    using (var query = new Query(session, queryString))
                    {
                        query.Execute();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Поле не найдено \"Код\"") && selectCode)
                    {
                        selectCode = false;
                        continue;
                    }

                    if (ex.Message.Contains("Поле не найдено \"Наименование\"") && selectName)
                    {
                        selectName = false;
                        continue;
                    }

                    // TODO: надо повнимательнее проверить - стоит проверять это поле при инициализации
                    // или все таки нужно как сейчас выполнять проверку при чтении справочника
                    //if (ex.Message.Contains("Поле не найдено \"ЭтоГруппа\"") && hierarchical)
                    //{
                    //    hierarchical = false;
                    //    continue;
                    //}

                    throw new Exception(string.Format("AddDocumentCommonRequisites failed: " + ex.Message));
                }
            }

            if (selectCode)
                reqViewList.Add(
                    new RequisiteView(RequisiteView.REQUISITE_CODE_ID, "Код", "Код", true, TypeInfo.StringType));

            if (selectName)
                reqViewList.Add(
                    new RequisiteView(RequisiteView.REQUISITE_NAME_ID, "Наименование", "Наименование", true, TypeInfo.StringType));
        }

        #region Работа со словарем отображений документов

        /// <summary>
        /// Путь к файлу сериализованного словаря отображений документов
        /// </summary>
        private string _documentViewFilePath;

        /// <summary>
        /// Объект синхронизации работы с словарем отображений документов
        /// </summary>
        private static readonly object s_documentViewDictSync = new object();

        /// <summary>
        /// Словарь отображений документов: [имя документа, описатель отображения документа]
        /// </summary>
        private Dictionary<string, DocumentView> _documentViewDict = new Dictionary<string, DocumentView>();

        /// <summary>
        /// Инициализация пути к файлу сериализованного словаря отображений документов
        /// </summary>
        private void InitDocumentViewFilePath()
        {
            _documentViewFilePath = Path.Combine(
                FileHelper.RootDirPath,
                Application.Instance.StubMode ? "Stub/documentview.bin" : "documentview.bin");
        }

        /// <summary>
        /// Сохранение словаря отображений документов в файл
        /// </summary>
        /// <param name="documentViewDict"></param>
        private void SaveDocumentViewDict(Dictionary<string, DocumentView> documentViewDict)
        {
            try
            {
                using (var stream = File.Open(_documentViewFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, documentViewDict);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Document view dictionary saving failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Загрузка словаря отображений документов
        /// </summary>
        private void LoadDocumentViewDict()
        {
            try
            {
                // если файл не найден
                if (!File.Exists(_documentViewFilePath))
                {
                    // выходим - инициализация словаря будет выполнена при первом обращении к нему
                    Logger.Trace("LoadDocumentViewDict: document view file '{0}' not found", _documentViewFilePath);
                    return;
                }

                // десериализуем словарь
                using (var stream = File.Open(_documentViewFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var formatter = new BinaryFormatter();
                    _documentViewDict = (Dictionary<string, DocumentView>)formatter.Deserialize(stream);
                }

                Logger.Trace("LoadDocumentViewDict: done");
            }
            catch (Exception ex)
            {
                throw new Exception("Document view dictionary loading failed: " + ex.Message);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}