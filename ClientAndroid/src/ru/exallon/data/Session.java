package ru.exallon.data;

import ru.exallon.Application;
import ru.exallon.server.ServiceProxy;

/**
 * Сессия работы с сервером

 */
public class Session 
{
	private static String SUB_LOG_TAG = "Session";
	
	private ServiceProxy _serviceProxy;
	/**
	 * Прокси для взаимодействия с сервером
	 */	
	public ServiceProxy getServiceProxy()
	{
		return _serviceProxy;
	}
	
	private String _id;
	/**
	 * Идентификатор сессии
	 */
	public String getId()
	{
		return _id;
	}
	
	private DatabaseSettings _dbSettings;
	/**
	 * Настройки БД, для подключения к которой создана сессия
	 */
	public DatabaseSettings getDbSettings()
	{
		return _dbSettings;
	}
	
	private boolean _open;
	/**
	 * Открыта ли данная сессия
	 */
	public boolean isOpen()
	{
		return _open;
	}
	
	private boolean _onError;
	/**
	 * Была ли сессия закрыта из-за ошибки, 
	 * которая возникла при обращении к серверу
	 */
	public boolean onError()
	{
		return _onError;
	}
	
	/**
	 * Закрыть сессию
	 * @param onError признак, что сессия была закрыта в результате ошибки, 
	 * которая возникла при обращении к серверу
	 */
	public void close(boolean onError)
	{
		_onError = onError;
		
		if (!_open)
			return;
		
		// сбрасываем ссылку на прокси, т.к. его уже точно не будем использовать
		_serviceProxy = null;
		_open = false;
	}
	
	/**
	 * Возвращает пустую закрытую сессию
	 * @return
	 */
	public static Session getNullSession()
	{
		Session s = new Session("0", null, null);
		s.close(false);
		return s;
	}
	
	/**
	 * Конструктор
	 * @param proxy
	 * @param dbSettings
	 * @param serverId
	 */
	public Session(String id, DatabaseSettings dbSettings, ServiceProxy proxy)
	{
		if (dbSettings != null)
			Application.logInfo(SUB_LOG_TAG, "Create Session: id=%s; serverId=%s", id, dbSettings.Id);
		
		_serviceProxy = proxy;
		_id = id;
		_dbSettings = dbSettings;
		_open = true;
	}
}
