package ru.exallon.data;

import android.content.OperationApplicationException;
import ru.exallon.Application;
import ru.exallon.server.SessionResponse;
import ru.exallon.server.DataResponse;
import ru.exallon.server.Response;
import ru.exallon.server.ServerError;
import ru.exallon.server.ServiceProxy;

public class SessionManager 
{
	private static String SUB_LOG_TAG = "SessionManager";
	
	private static SessionManager _instance;
	
	/**
	 * Экземпляр класса
	 */
	public static synchronized SessionManager getInstance()
	{
		if (_instance == null)
			_instance = new SessionManager();
		
		return _instance;
	}
	
	/**
	 * Закрытый конструктор
	 */
	private SessionManager()
	{
	}
	
	/**
	 * Активная сессия
	 */
	private Session _activeSession = Session.getNullSession(); // инициализируем пустой закрытой сессией
	
	/**
	 * Идентификатор активного сервера, т.е. сервера, 
	 * с которым сейчас выполняется взаимодействие
	 * @throws OperationApplicationException 
	 */
	public String getActiveServerId() throws Exception
	{		
		return _activeSession.getDbSettings().Id.toString();
	}
	
	/**
	 * Открыть новую сессию для работы с сервером
	 * @param dbs настройка доступа к БД
	 * @return
	 */
	public SessionResponse openSession(DatabaseSettings dbs)
	{
		Application.logVerbose(SUB_LOG_TAG, "openSession: ServerUrl=%s; Username=%s", 
				dbs.ServerUrl, dbs.Username);
		
		// если активная сессия открыта
		if (_activeSession.isOpen())
		{
			DatabaseSettings activeSessionDbs = _activeSession.getDbSettings();
			
			// и активная сессия - это сессия к тому же серверу
			if (activeSessionDbs.equals(dbs))
				// то ничего не делаем
				return new SessionResponse(activeSessionDbs.Id.toString());
			
			// закрываем текущую открытую сессию
			closeSession(false);
		}
		
		// открываем новую
		ServiceProxy proxy = new ServiceProxy(DatabaseSettings.formatServerAddresss(dbs.ServerUrl));
    	SessionResponse resp = proxy.Authorize(dbs.Username, dbs.Password);
    	
    	// если ошибка есть
    	if (checkError(resp, false))
    		// то сразу возвращаем ответ, без создания новой сессии
    		return resp;

    	_activeSession = new Session(resp.getSessionId(), dbs, proxy);
    	
		return resp;
	}
	
	/**
	 * Закрыть текущую активную сессию
	 */
	public SessionResponse closeSession()
	{
		return closeSession(false);
	}
	
	/**
	 * Получить список справочников
	 * @return
	 * @throws Exception 
	 */
	public DataResponse getCatalogs() 
	{
		Application.logVerbose(SUB_LOG_TAG, "getCatalogs");
		
		return getDataFromServer(new ServerCallHandler() 
		{
			@Override
			public DataResponse getData(String sessionId) 
			{
				return _activeSession.getServiceProxy().GetCatalogs(sessionId);
			}
		});
	}
	
	/**
	 * Получить элементы справочника
	 * @param catalogName имя справочника
	 * @param indexFrom Индекс элемента, с которого должна начаться порция данных
	 * @param indexTo Индекс элемента, которым должна закончиться порция данных
	 * @return
	 * @throws Exception 
	 */
	public DataResponse getCatalogItems(
			final String catalogName, final int indexFrom, final int indexTo, final String parentId)
	{
		Application.logVerbose(SUB_LOG_TAG, 
				"getCatalogs: catalogName=%s; indexFrom=%d; indexTo=%d; parentId=%s",
				catalogName, indexFrom, indexTo, parentId);
		
		return getDataFromServer(new ServerCallHandler() 
		{
			@Override
			public DataResponse getData(String sessionId) 
			{
				return _activeSession.getServiceProxy().GetCatalogItems(
						sessionId,
						catalogName,
						indexFrom, 
						indexTo,
						parentId);
			}
		});
	}
	
	/**
	 * Получить все элементы справочника
	 * @param catalogName имя справочника
	 * @return
	 * @throws Exception 
	 */
	public DataResponse getAllCatalogItems(final String catalogName)
	{
		Application.logVerbose(SUB_LOG_TAG, "getAllCatalogItems: catalogName='%s'", catalogName);
					
		return getDataFromServer(new ServerCallHandler() 
		{
			@Override
			public DataResponse getData(String sessionId) 
			{
				return _activeSession.getServiceProxy().GetAllCatalogItems(sessionId, catalogName);
			}
		});
	}
	
	/**
	 * Получить детальную информацию по элементу справочника
	 * @param catalogName имя (идентификатор справочника)
	 * @param itemId идентификатор элемента справочника
	 * @return
	 */
	public DataResponse getCatalogItemDetails(final String catalogName, final String itemId)
	{
		Application.logVerbose(SUB_LOG_TAG, 
				"GetCatalogItemDetails: catalogName=%s; itemId=%s", catalogName, itemId);
		
		return getDataFromServer(new ServerCallHandler() 
		{
			@Override
			public DataResponse getData(String sessionId) 
			{
				return _activeSession.getServiceProxy().GetCatalogItemDetails(sessionId, catalogName, itemId);
			}
		});
	}
	
	/**
	 * Получить список документов
	 * @return
	 * @throws Exception 
	 */
	public DataResponse getDocuments() 
	{
		Application.logVerbose(SUB_LOG_TAG, "getDocuments");
		
		return getDataFromServer(new ServerCallHandler() 
		{
			@Override
			public DataResponse getData(String sessionId) 
			{
				return _activeSession.getServiceProxy().GetDocuments(sessionId);
			}
		});
	}
	
	/**
	 * Интерфейс вызова метода сервера
	 * @author akonyaev
	 *
	 */
	private interface ServerCallHandler
	{
		DataResponse getData(String sessionId);
	}
	
	/**
	 * Вызов метода сервера для получения данных
	 * @param handler
	 * @return
	 */
	private DataResponse getDataFromServer(ServerCallHandler handler)
	{
		// если сессия закрыта
		if (!_activeSession.isOpen())
			return DataResponse.getFailedResponse(ServerError.CLIENT_NO_CONNECTION);
			
		DataResponse resp = handler.getData(_activeSession.getId());
		
		// если ответ содержит ошибку
		if (checkError(resp, true))
		{			
			// попробуем подключиться к серверу снова
			SessionResponse sr = openSession(_activeSession.getDbSettings());
			
			// если подключиться так и не удалось
			if (sr.hasError())
				return DataResponse.getFailedResponse(ServerError.CLIENT_NO_CONNECTION);
			
			// подключились => попробуем вызвать сервер еще раз
			resp = handler.getData(_activeSession.getId());
			checkError(resp, true);
		}
		
		return resp;
	}
	
	/**
	 * Проверка ответа на наличие ошибки
	 * @param resp ответ, который проверяется на содержание ошибки
	 * @param closeSession нужно ли закрывать сессию, если есть ошибка
	 * @return false - ошибки нет, true - ошибка была обработана
	 */
	private boolean checkError(Response resp, boolean closeSession)
	{
		// если ошибки нет
		if (!resp.hasError())
			// ничего не делаем
			return false;
		
		if (closeSession)
		{
			// закрываем сессию
			closeSession(true);
		}
		
		// пишем в лог
		ServerError error = resp.getError();
		int code = error.getCode();
		Application.logError(SUB_LOG_TAG, "Ошибка [%d]: %s", code, error.getDescription());
		
		// заменим описание ошибки на более подходящее для отображения пользователю
		resp.setError(code);
		
		return true;
	}
	
	/**
	 * Закрыть сессию
	 * @param onError
	 * @return
	 */
	private SessionResponse closeSession(boolean onError)
	{
		String activeSessionId = _activeSession.getId();
		Application.logVerbose(SUB_LOG_TAG, "closeSession: sessionId=%s", activeSessionId);
		
		// если сессия уже и так закрыта
		if (!_activeSession.isOpen())
		{
			return new SessionResponse(activeSessionId); 
		}
		
		// закрываем сессию на сервере
    	SessionResponse resp = _activeSession.getServiceProxy().Close(activeSessionId);
    	
    	// проверяем на ошибку
    	checkError(resp, false);
    	
		// закрываем сессию
		_activeSession.close(onError);
    	
    	return resp;
	}
}
