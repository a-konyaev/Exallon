using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using Enterra.V8x1C.DOM;
using Exallon.Configurations;
using Exallon.Sessions;
using Exallon.Utils;

namespace Exallon.Data
{
    /// <summary>
    /// Сервис работы с данными
    /// </summary>
    [ServiceBehavior(Namespace = "http://www.exallon.ru", InstanceContextMode = InstanceContextMode.PerSession)]
    public class DataService : IDataService
    {
        private readonly IDataManager _dataManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly ISessionManager _sessionManager;
        private readonly NLog.Logger _logger;

        public DataService()
        {
            // получаем ссылки на нужные компоненты системы
            _dataManager = Application.Instance.GetSubsystem<IDataManager>();
            _configurationManager = Application.Instance.GetSubsystem<IConfigurationManager>();
            _sessionManager = Application.Instance.GetSubsystem<ISessionManager>();

            _logger = _dataManager.Logger;
        }

        #region IDataService Members

        #region Авторизация и закрытие сессии

        /// <summary>
        /// Авторизация в системе
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        /// <returns>результат выполнения операции</returns>
        public SessionResponse Authorize(string login, string password)
        {
            _logger.Info("{0}->Authorize: login='{1}'", NetHelper.GetClientAddress(), login);

            try
            {
                var sessionId = _sessionManager.CreateNewSession(login, password);
                return new SessionResponse(sessionId);
            }
            catch (LicenseException)
            {
                _logger.Warn("Authorize: license expired or not exist");
                return new SessionResponse(Response.ERROR_CODE_LICENSE_EXPIRED);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Идентификация пользователя не выполнена"))
                {
                    _logger.Warn("Authorize: unauthorized access");
                    return new SessionResponse(Response.ERROR_CODE_UNAUTHORIZED);
                }

                _logger.Error("Authorize: authorization failed", ex);
                return new SessionResponse(Response.ERROR_CODE_UNKNOWN_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Закрытие сессии
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        public SessionResponse Close(string sessionId)
        {
            _logger.Info("{0}->Close: sessionId='{1}'", NetHelper.GetClientAddress(), sessionId);

            try
            {
                _sessionManager.CloseSession(sessionId);
                return new SessionResponse(sessionId);
            }
            catch (Exception ex)
            {
                _logger.Error("Close: failed", ex);
                return new SessionResponse(Response.ERROR_CODE_UNKNOWN_ERROR, ex.Message);
            }
        }

        #endregion

        #region Справочники

        /// <summary>
        /// Получение списка справочников
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        public GetDataResponse GetCatalogs(string sessionId)
        {
            _logger.Info("{0}->GetCatalogs: sessionId='{1}'", NetHelper.GetClientAddress(), sessionId);

            try
            {
                // сессия не нужна, но получим ее, чтобы проверить, что она есть
                _sessionManager.GetSession(sessionId);

                // получим список отображений справочников, которые нужно отображать
                var catsView = _configurationManager.GetCatalogsView().Where(catView => catView.Show);
                var data = catsView.Select(i => new DataItem(i.Name, i.Presentation, null)).ToArray();

                _logger.Trace("GetCatalogs: returns {0} items", data.Length);
                return new GetDataResponse(data);
            }
            catch (SessionNotFoundException)
            {
                return new GetDataResponse(Response.ERROR_CODE_SESSION_NOT_FOUND);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("GetCatalogs: failed", ex);
                return new GetDataResponse(Response.ERROR_CODE_UNKNOWN_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Получение элементов справочника
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <param name="catalogName">имя справочника</param>
        /// <param name="indexFrom">индекс элемента, начиная с которого нужно вернуть данные</param>
        /// <param name="indexTo">индекс элемента, заканчивая которым нужно вернуть данные</param>
        /// <param name="parentId">идентификатор родительского элемента (для иерархических справочников)</param>
        /// <returns></returns>
        public GetDataResponse GetCatalogItems(
            string sessionId,
            string catalogName,
            int indexFrom,
            int indexTo,
            string parentId)
        {
            _logger.Info(
                "{0}->GetCatalogItems: sessionId='{1}'; catalogName='{2}'; parentId='{3}'; indexFrom='{4}'; indexTo='{5}'",
                NetHelper.GetClientAddress(), sessionId, catalogName, parentId, indexFrom, indexTo);

            if (string.IsNullOrEmpty(catalogName))
                return new GetDataResponse(Response.ERROR_CODE_UNKNOWN_ERROR, "Catalog's name has not set");

            try
            {
                return _sessionManager.ExecuteWithinSession(
                    sessionId,
                    session =>
                        {
                            var items = new List<DataItem>();

                            ReadCatalog(
                                session,
                                catalogName,
                                _configurationManager.GetCatalogView(catalogName),
                                true,
                                indexFrom < 0 ? null : (int?) indexFrom,
                                indexTo < 0 ? null : (int?) indexTo,
                                parentId,
                                (id, code, name, itemParentId, isGroup) =>
                                    items.Add(new DataItem(id, name, null, itemParentId, isGroup)));

                            var data = items.ToArray();
                            _logger.Trace("GetCatalogItems: returns {0} items", data.Length);

                            return new GetDataResponse(data);
                        });
            }
            catch (SessionNotFoundException)
            {
                return new GetDataResponse(Response.ERROR_CODE_SESSION_NOT_FOUND);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("GetCatalogItems: failed", ex);
                return new GetDataResponse(Response.ERROR_CODE_UNKNOWN_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Получить все элементы справочника
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="catalogName"></param>
        /// <returns></returns>
        public GetDataResponse GetAllCatalogItems(string sessionId, string catalogName)
        {
            _logger.Info("{0}->GetAllCatalogItems: sessionId='{1}'; catalogName='{2}'",
                NetHelper.GetClientAddress(), sessionId, catalogName);

            if (string.IsNullOrEmpty(catalogName))
                return new GetDataResponse(Response.ERROR_CODE_UNKNOWN_ERROR, "Catalog's name has not set");

            try
            {
                return _sessionManager.ExecuteWithinSession(
                    sessionId,
                    session =>
                        {
                            var items = new List<DataItem>();

                            ReadCatalog(
                                session,
                                catalogName,
                                _configurationManager.GetCatalogView(catalogName),
                                false,
                                null,
                                null,
                                null,
                                (id, code, name, itemParentId, isGroup) =>
                                    items.Add(new DataItem(id, name, null, itemParentId, isGroup)));

                            var data = items.ToArray();
                            _logger.Trace("GetAllCatalogItems: returns {0} items", data.Length);

                            return new GetDataResponse(data);
                        }
                    );
            }
            catch (SessionNotFoundException)
            {
                return new GetDataResponse(Response.ERROR_CODE_SESSION_NOT_FOUND);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("GetAllCatalogItems: failed", ex);
                return new GetDataResponse(Response.ERROR_CODE_UNKNOWN_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Получить детальную информацию по элементу данных
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <param name="catalogName">имя справочника</param>
        /// <param name="itemId">идентификатор элемента данных</param>
        /// <returns></returns>
        public GetDataResponse GetCatalogItemDetails(string sessionId, string catalogName, string itemId)
        {
            _logger.Info("{0}->GetCatalogItemDetails: sessionId='{1}'; catalogName='{2}'; itemId='{3}'",
                NetHelper.GetClientAddress(), sessionId, catalogName, itemId);

            if (string.IsNullOrEmpty(itemId))
                return new GetDataResponse(
                    Response.ERROR_CODE_UNKNOWN_ERROR, "Item's id has not set");

            try
            {
                return _sessionManager.ExecuteWithinSession(
                    sessionId,
                    session =>
                        {
                            // получим список реквизитов, которые нужно отображать
                            var requisites =
                                _configurationManager.GetCatalogView(catalogName).Requisites.Where(req => req.Show).
                                    ToList();

                            // читаем реквизиты справочника
                            var items = new List<DataItem>();
                            ReadCatalogItemRequisites(
                                session,
                                catalogName,
                                requisites,
                                itemId,
                                (index, name, presentation, value) =>
                                items.Add(new DataItem(name, presentation, value)));

                            var data = items.ToArray();
                            _logger.Trace("GetCatalogItemDetails: returns {0} items", data.Length);

                            return new GetDataResponse(data);
                        });
            }
            catch (SessionNotFoundException)
            {
                return new GetDataResponse(Response.ERROR_CODE_SESSION_NOT_FOUND);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("GetCatalogItemDetails: failed", ex);
                return new GetDataResponse(Response.ERROR_CODE_UNKNOWN_ERROR, ex.Message);
            }
        }

        #endregion

        #region Документы

        /// <summary>
        /// Получение списка документов
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        public GetDataResponse GetDocuments(string sessionId)
        {
            _logger.Info("{0}->GetDocuments: sessionId='{1}'", NetHelper.GetClientAddress(), sessionId);

            try
            {
                // сессия не нужна, но получим ее, чтобы проверить, что она есть
                _sessionManager.GetSession(sessionId);

                // получим список отображений документов, которые нужно отображать
                var docsView = _configurationManager.GetDocumentsView().Where(view => view.Show);
                var data = docsView.Select(i => new DataItem(i.Name, i.Presentation, null)).ToArray();

                _logger.Trace("GetDocuments: returns {0} items", data.Length);
                return new GetDataResponse(data);
            }
            catch (SessionNotFoundException)
            {
                return new GetDataResponse(Response.ERROR_CODE_SESSION_NOT_FOUND);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("GetDocuments: failed", ex);
                return new GetDataResponse(Response.ERROR_CODE_UNKNOWN_ERROR, ex.Message);
            }
        }

        #endregion

        #endregion

        #region ReadCatalog

        /// <summary>
        /// Пустой идентификатор
        /// </summary>
        static readonly string s_emptyId = Guid.Empty.ToString("D");
        
        /// <summary>
        /// Делегат метода обработки запроса чтения справочника
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="isGroup"></param>
        private delegate void ProcessReadCatalogQueryResult(
            string id, string code, string name, string parentId, bool isGroup);

        /// <summary>
        /// Выполнение запроса чтения справочника
        /// </summary>
        /// <param name="session"></param>
        /// <param name="catalogName"></param>
        /// <param name="catalogView"></param>
        /// <param name="asTree"></param>
        /// <param name="indexFrom"></param>
        /// <param name="indexTo"></param>
        /// <param name="parentId"></param>
        /// <param name="processResult"> </param>
        /// <returns></returns>
        private static void ReadCatalog(
            Session session,
            string catalogName,
            CatalogView catalogView,
            bool asTree, 
            int? indexFrom, 
            int? indexTo,
            string parentId, 
            ProcessReadCatalogQueryResult processResult)
        {
            if (Application.Instance.StubMode)
            {
                ReadCatalogStub(catalogName, parentId, processResult);
                return;
            }

            string queryString = null;
            QueryResult queryResult;
            var selectCode = catalogView.HasCodeRequisite;
            var selectName = catalogView.HasNameRequisite;
            var hierarchical = session.Catalogs[catalogName].Metadata.Hierarchical;

            while (true)
            {
                try
                {
                    // сформируем строку запроса
                    queryString = FormatReadCatalogQueryString(
                        catalogName, selectCode, selectName, hierarchical, asTree, indexFrom, indexTo);

                    // создаем запрос
                    using (var query = new Query(session, queryString))
                    {
                        // если нужно вернуть данные в виде дерева
                        if (asTree)
                        {
                            // добавляем параметр "Родительский элемент"
                            parentId = (string.IsNullOrEmpty(parentId) ? s_emptyId : parentId);
                            query.SetParameter("parent", new CatalogRef(session, catalogName, parentId));
                        }

                        // выполняем запрос
                        queryResult = query.Execute();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (hierarchical && ex.Message.Contains("Поле не найдено \"a.ЭтоГруппа\""))
                    {
                        hierarchical = false;
                        continue;
                    }

                    throw new Exception(
                        string.Format("Error of executing query '{0}' for read catalog '{1}': {2}",
                        queryString, catalogName, ex.Message));
                }
            }

            var valueTable = queryResult.Unload();

            BaseObject.RestDelay();

            var dt = valueTable.ToDataTable(true, valueTable.LoadCache());

            // получаем элементы справочника
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];

                var itemId = ((CatalogRef)row["Ссылка"]).Uuid.ToString();
                var itemCode = (selectCode ? row["Код"].ToString() : null);
                var itemName = (selectName ? row["Наименование"].ToString() : null);
                var itemParentId = (hierarchical ? ((CatalogRef)row["Родитель"]).Uuid.ToString() : s_emptyId);
                var itemIsGroup = (hierarchical && (bool)row["ЭтоГруппа"]);

                processResult(itemId, itemCode, itemName, itemParentId, itemIsGroup);
            }
        }

        /// <summary>
        /// Сформировать текст запроса для чтения справочника
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="selectCode"></param>
        /// <param name="selectName"></param>
        /// <param name="hierarchical"></param>
        /// <param name="asTree"></param>
        /// <param name="indexFrom">индекс элемента, начиная с которого нужно вернуть данные</param>
        /// <param name="indexTo">индекс элемента, заканчивая которым нужно вернуть данные</param>
        /// <returns></returns>
        private static string FormatReadCatalogQueryString(
            string catalogName,
            bool selectCode,
            bool selectName,
            bool hierarchical,
            bool asTree,
            int? indexFrom,
            int? indexTo)
        {
            var queryBuilder = new StringBuilder(2048);

            // задан ли диапазон номеров элементов
            var rangeDefined = indexFrom.HasValue || indexTo.HasValue;

            if (rangeDefined)
                // внешний SELECT для ограничения кол-ва выбираемых элементов в соотв. с номером страницы
                queryBuilder.Append("SELECT t.* FROM (");

            #region Основной SELECT

            // SELECT-----------------------------------------
            queryBuilder.Append("SELECT a.Ссылка");

            if (selectCode)
                queryBuilder.Append(", a.Код");

            if (selectName)
                queryBuilder.Append(", a.Наименование");

            if (hierarchical)
                queryBuilder.Append(", a.Родитель, a.ЭтоГруппа");

            if (rangeDefined)
                queryBuilder.Append(", КОЛИЧЕСТВО(a2.Ссылка) AS RowNum");

            // FROM-------------------------------------------
            queryBuilder.AppendFormat(" FROM Справочник.{0} AS a", catalogName);

            if (rangeDefined)
            {
                // TODO: попробовать переписать запрос через использование временной таблицы - может быстрее будет работать
                queryBuilder.AppendFormat(" LEFT JOIN Справочник.{0} AS a2 ON a.Ссылка > a2.Ссылка", catalogName);

                if (hierarchical)
                    queryBuilder.Append(" AND a2.Родитель = a.Родитель");
            }

            // WHERE------------------------------------------
            if (asTree)
                queryBuilder.Append(" WHERE a.Родитель = &parent");

            if (hierarchical || selectName)
            {
                queryBuilder.Append(" ORDER BY ");

                if (hierarchical)
                    queryBuilder.Append("a.ЭтоГруппа");

                if (selectName)
                {
                    if (hierarchical)
                        queryBuilder.Append(", ");

                    queryBuilder.Append("a.Наименование");
                }
            }

            // GROUP BY---------------------------------------
            if (rangeDefined)
            {
                queryBuilder.Append(" GROUP BY a.Ссылка");

                if (selectCode)
                    queryBuilder.Append(", a.Код");

                if (selectName)
                    queryBuilder.Append(", a.Наименование");

                if (hierarchical)
                    queryBuilder.Append(", a.Родитель, a.ЭтоГруппа");
                //queryBuilder.Append(groupListBuilder.ToString());
            }

            #endregion

            if (rangeDefined)
            {
                // добавим условие для выбора строк, номера которых соотв. заданному диапазону
                queryBuilder.Append(") AS t WHERE");

                if (indexFrom.HasValue)
                {
                    queryBuilder.Append(" t.RowNum >= ");
                    queryBuilder.Append(indexFrom.Value);
                }

                if (indexTo.HasValue)
                {
                    if (indexFrom.HasValue)
                        queryBuilder.Append(" AND");

                    queryBuilder.Append(" t.RowNum <= ");
                    queryBuilder.Append(indexTo.Value);
                }
            }

            return queryBuilder.ToString();
        }

        #endregion

        #region ReadCatalogItemRequisites

        /// <summary>
        /// Делегат метода обработки запроса чтения реквизитов элемента справочника
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="presentation"></param>
        /// <param name="value"></param>
        private delegate void ReadCatalogItemRequisitesProcessResult(
            int index, string name, string presentation, string value);

        /// <summary>
        /// Чтение реквизитов элемента справочника
        /// </summary>
        /// <param name="session"></param>
        /// <param name="catalogName"></param>
        /// <param name="requisites"></param>
        /// <param name="itemId"></param>
        /// <param name="processResult"> </param>
        private static void ReadCatalogItemRequisites(
            Session session,
            string catalogName,
            IList<RequisiteView> requisites,
            string itemId,
            ReadCatalogItemRequisitesProcessResult processResult)
        {
            if (Application.Instance.StubMode)
            {
                ReadCatalogItemRequisitesStub(catalogName, requisites, itemId, processResult);
                return;
            }

            DataTable dt;
            var queryString = FormatQueryStringForReadCatalogItemRequisites(session, catalogName, requisites);

            using (var query = new Query(session, queryString))
            {
                query.SetParameter("itemId", new CatalogRef(session, catalogName, itemId));

                // выполняем запрос
                var queryResult = query.Execute();
                var valueTable = queryResult.Unload();
                dt = valueTable.ToDataTable(true, valueTable.LoadCache());
            }

            if (dt.Rows.Count != 1)
            {
                throw new Exception("query returned not exactly one row");
            }

            // получаем элементы справочника
            var row = dt.Rows[0];

            foreach (var req in requisites)
            {
                BaseObject.RestDelay();
                var reqName = req.Name;

                // если справочник содержит поля типа Unknown
                // (это может быть, например, поле типа "ХранилищеЗначения" - какой то кастомный тип),
                // то в таблице его не будет
                var reqValue = !dt.Columns.Contains(reqName)
                                   ? "Ссылка на объект"
                                   : ConvertRequisiteValueToString(row[reqName], req.TypeInfo.Type);

                processResult(req.Id, reqName, req.Presentation, reqValue);
            }
        }

        /// <summary>
        /// Сформировать текст запроса для чтения детальной информации элемента справочника
        /// </summary>
        /// <param name="session"></param>
        /// <param name="catalogName"></param>
        /// <param name="requisites"></param>
        /// <returns></returns>
        private static string FormatQueryStringForReadCatalogItemRequisites(
            Session session,
            string catalogName,
            IEnumerable<RequisiteView> requisites)
        {
            var queryBuilder = new StringBuilder(4096);

            // SELECT-----------------------------------------
            queryBuilder.Append("SELECT a.Ссылка,");

            // добавим колонки, которые соответствуют реквизитам справочника
            var joinedReqs = new List<RequisiteView>(8);

            foreach (var req in requisites)
            {
                switch (req.TypeInfo.Type)
                {
                    case TypeEnum.EnumRef:
                    case TypeEnum.DocumentRef:
                        queryBuilder.AppendFormat("r{0}.Ссылка as {1},", joinedReqs.Count, req.Name);
                        joinedReqs.Add(req);
                        break;

                    case TypeEnum.CatalogRef:
                        var refCatalog = session.Catalogs[req.TypeInfo.ReferenceTypeName];
                        var colName = (refCatalog.Metadata.NameLength > 0 ? "Наименование" : "Код");
                        queryBuilder.AppendFormat("r{0}.{1} as {2},", joinedReqs.Count, colName, req.Name);
                        joinedReqs.Add(req);
                        break;

                    default:
                        queryBuilder.AppendFormat("a.{0},", req.Name);
                        break;
                }
            }

            queryBuilder.Length -= 1;

            #region Попытка возвращать все данные в 1 колонке

            // тут нужно не просто делать конкатенацию, а каждую колонку проверять на NULL и если она NULL, 
            // то добавлять типа <NULL>. 
            // Ну и преобразование типов к строке надо побороть: пока не понятно, как Ссылка привести к Строка. 
            // вообще пишут, что концепция 1С такова, что типы нельзя преобразовывать в запросах...

            //for (var i = 0; i < requisites.Count; i++)
            //{
            //    var req = requisites[i];
            //    var typeInfo = req.Type.Types[0];

            //    switch (typeInfo.Type)
            //    {
            //        case TypeEnum.EnumRef:
            //            break;

            //        case TypeEnum.DocumentRef:
            //            queryBuilder.AppendFormat(" + \"&\" + r{0}.Код", joinedReqs.Count);
            //            joinedReqs.Add(req);
            //            break;

            //        case TypeEnum.CatalogRef:
            //            var refCatalog = GetCatalogManager(session, typeInfo.ReferenceTypeName);
            //            var colName = (refCatalog.Metadata.NameLength > 0 ? "Наименование" : "Код");
            //            queryBuilder.AppendFormat(" + \"&\" + r{0}.{1}", joinedReqs.Count, colName);
            //            joinedReqs.Add(req);
            //            break;

            //        case TypeEnum.String:
            //            queryBuilder.AppendFormat(" + \"&\" + ВЫРАЗИТЬ(a.{0} КАК СТРОКА(255))", req.Name);
            //            break;

            //        case TypeEnum.Boolean:
            //            queryBuilder.AppendFormat(" + \"&\" + \"B_{0}\"", req.Name);
            //            break;

            //        case TypeEnum.Date:
            //            queryBuilder.AppendFormat(" + \"&\" + \"D_{0}\"", req.Name);
            //            break;

            //        case TypeEnum.Decimal:
            //            queryBuilder.AppendFormat(" + \"&\" + \"Dc_{0}\"", req.Name);
            //            break;

            //        default:
            //            queryBuilder.AppendFormat(" + \"&\" + ВЫРАЗИТЬ(a.{0} КАК СТРОКА(255))", req.Name);
            //            break;
            //    }
            //}

            #endregion

            // FROM-------------------------------------------
            queryBuilder.AppendFormat(" FROM Справочник.{0} AS a", catalogName);

            // JOINs------------------------------------------
            for (var i = 0; i < joinedReqs.Count; i++)
            {
                var req = joinedReqs[i];
                var refType = GetTypeName(req.TypeInfo.Type);

                queryBuilder.AppendFormat(
                    " LEFT JOIN {0}.{1} AS r{2} ON a.{3} = r{2}.Ссылка",
                    refType, req.TypeInfo.ReferenceTypeName, i, req.Name);
            }

            // WHERE------------------------------------------
            queryBuilder.Append(" WHERE a.Ссылка = &itemId");

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Получить наименование типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTypeName(TypeEnum type)
        {
            switch (type)
            {
                case TypeEnum.DocumentRef:
                    return "Документ";
                case TypeEnum.CatalogRef:
                    return "Справочник";
                case TypeEnum.EnumRef:
                    return "Перечисление";

                default:
                    throw new Exception("Неожиданный тип: " + type);
            }
        }

        /// <summary>
        /// Преобразовать значение реквизита к строке
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string ConvertRequisiteValueToString(object value, TypeEnum type)
        {
            string valStr;

            if (value is DBNull)
            {
                return null;
            }

            switch (type)
            {
                case TypeEnum.Boolean:
                    return (bool)value ? "Да" : "Нет";

                case TypeEnum.EnumRef:
                    //TODO: надо получать значение поля Синоним
                    valStr = ((EnumRef)value).ValueName;
                    break;

                case TypeEnum.DocumentRef:
                    return string.Format("Ссылка на документ '{0}'", ((DocumentRef)value).ObjectTypeName);

                default:
                    valStr = value.ToString();
                    break;
            }

            return (string.Empty.Equals(valStr) ? null : valStr);
        }

        #endregion

        #region Заглушка

        /// <summary>
        /// Путь к файлу данных заглушки
        /// </summary>
        private static string s_stubDataFilePath;
        /// <summary>
        /// Xml-документ, содержащий данные заглушки
        /// </summary>
        private static XmlDocument s_stubXmlDoc;

        /// <summary>
        /// Инициализация заглушки
        /// </summary>
        public static void InitStub()
        {
            try
            {
                // получим полный путь к файлу данных заглушки
                s_stubDataFilePath = Path.Combine(FileHelper.RootDirPath, "Stub/data.xml");

                // NOTE: раскоментировать для заполнения заглушки данными
                // StubMode должен быть = False
                //DataService.FillStubData("111", "");

                s_stubXmlDoc = new XmlDocument();
                s_stubXmlDoc.Load(s_stubDataFilePath);
            }
            catch (Exception ex)
            {
                Application.Instance.Logger.FatalException("Init stub failed", ex);
                throw new Exception("InitStub failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Заглушка чтения справочника
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="parentId"></param>
        /// <param name="processResult"></param>
        private static void ReadCatalogStub(
            string catalogName,
            string parentId,
            ProcessReadCatalogQueryResult processResult)
        {
            var sb = new StringBuilder(128);
            sb.AppendFormat("//catalog[@name='{0}']/items/item", catalogName);
            
            if (!string.IsNullOrEmpty(parentId))
                sb.AppendFormat("[@parentId='{0}']", parentId);

            var nodeList = s_stubXmlDoc.SelectNodes(sb.ToString());
            if (nodeList == null)
                return;

            foreach (XmlNode node in nodeList)
            {
                var id = node.Attributes["id"].Value;
                var code = node.Attributes["code"].Value;
                var name = node.Attributes["name"].Value;
                var itemParentId = node.Attributes["parentId"].Value;
                var isGroup = bool.Parse(node.Attributes["isGroup"].Value);

                processResult(id, code, name, itemParentId, isGroup);
            }
        }

        /// <summary>
        /// Заглушка чтения реквизитов элемента справочника
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="requisites"></param>
        /// <param name="itemId"></param>
        /// <param name="processResult"></param>
        private static void ReadCatalogItemRequisitesStub(
            string catalogName,
            IList<RequisiteView> requisites,
            string itemId,
            ReadCatalogItemRequisitesProcessResult processResult)
        {
            var xpathReqs = string.Format("//catalog[@name='{0}']/items/item[@id='{1}']/req", catalogName, itemId);

            var nodeList = s_stubXmlDoc.SelectNodes(xpathReqs);
            if (nodeList == null)
                return;

            foreach (XmlNode node in nodeList)
            {
                var id = int.Parse(node.Attributes["id"].Value);
                var value = node.InnerText;
                if (string.Empty.Equals(value))
                    value = null;

                var req = requisites.FirstOrDefault(r => r.Id == id);

                if (req != null)
                    processResult(0, req.Name, req.Presentation, value);
            }
        }

        /// <summary>
        /// Заполнение заглушки данными
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль пользователя</param>
        public static void FillStubData(string login, string password)
        {
            var sessionManager = Application.Instance.GetSubsystem<ISessionManager>();
            var configurationManager = Application.Instance.GetSubsystem<IConfigurationManager>();
            var logger = Application.Instance.GetSubsystem<IDataManager>().Logger;

            try
            {
                logger.Trace("FillStubData: run...");

                var session = sessionManager.GetSession(sessionManager.CreateNewSession(login, password));
                var catalogs = session.Metadata.Catalogs;

                var doc = new XmlDocument();
                var root = doc.CreateElement("catalogs");
                doc.AppendChild(root);

                var cur = 0;
                var all = catalogs.Count;
                var percent = 0;

                foreach (MetadataObject catalog in catalogs)
                {
                    var catalogElem = root.AddChildElement("catalog");

                    var catalogName = catalog.Name;
                    catalogElem.AddAttribute("name", catalogName);
                    catalogElem.AddAttribute("presentation", catalog.Presentation);

                    var catalogView = configurationManager.GetCatalogView(catalogName);
                    var reqs = catalogView.Requisites;

                    // добавляем перечень реквизитов
                    var reqsElem = catalogElem.AddChildElement("requisites");
                    foreach (var req in reqs)
                    {
                        var reqElem = reqsElem.AddChildElement("requisite");
                        reqElem.AddAttribute("id", req.Id.ToString(CultureInfo.InvariantCulture));
                        reqElem.AddAttribute("name", req.Name);
                        reqElem.AddAttribute("presentation", req.Presentation);
                        reqElem.AddAttribute("type", req.TypeInfo.Type.ToString());
                    }

                    // добавляем элементы
                    var itemsElem = catalogElem.AddChildElement("items");
                    ReadCatalog(
                        session,
                        catalogName,
                        catalogView,
                        false,
                        null,
                        null,
                        null,
                        (id, code, name, itemParentId, isGroup) =>
                            {
                                var itemElem = itemsElem.AddChildElement("item");
                                itemElem.AddAttribute("id", id);
                                itemElem.AddAttribute("code", code);
                                itemElem.AddAttribute("name", name);
                                itemElem.AddAttribute("parentId", itemParentId);
                                itemElem.AddAttribute("isGroup", isGroup.ToString(CultureInfo.InvariantCulture));

                                ReadCatalogItemRequisites(
                                    session, 
                                    catalogName, 
                                    reqs, 
                                    id, 
                                    (reqIndex, reqName, reqPresentation, reqValue) =>
                                        {
                                            var reqElem = itemElem.AddChildElement("req");
                                            reqElem.AddAttribute("id", reqIndex.ToString(CultureInfo.InvariantCulture));
                                            reqElem.InnerText = reqValue;
                                        });
                            });

                    var newPercent = (++cur*100/all);
                    if (newPercent != percent)
                    {
                        percent = newPercent;
                        logger.Trace("FillStubData: {0}%", percent);
                    }
                }

                doc.Save(s_stubDataFilePath);
                logger.Trace("FillStubData: done!");
            }
            catch (Exception ex)
            {
                throw new Exception("Stub data filling failed: " + ex.Message);
            }
        }

        #endregion
    }
}