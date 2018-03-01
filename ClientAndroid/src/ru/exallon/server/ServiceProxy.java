package ru.exallon.server;

import java.net.SocketTimeoutException;
import java.net.UnknownHostException;

import org.ksoap2.*;
import org.ksoap2.transport.HttpTransportSE;
import org.ksoap2.serialization.SoapObject;
import org.ksoap2.serialization.SoapSerializationEnvelope;
import org.ksoap2.serialization.PropertyInfo;

import ru.exallon.Application;
import ru.exallon.data.DataItem;

/**
 * Прокси для доступа к сервису
 */
public class ServiceProxy
{
	private static String SUB_LOG_TAG = "ServiceProxy";
	private final int TIMEOUT = 15000; // 15 сек
	private final int TIMEOUT_SHORT = 3000; // 3 сек
	
	/**
	 * Адрес веб-сервиса
	 */
	private String _address;
	
	/**
	 * Конструктор
	 * @param address адрес веб-сервера
	 */
	public ServiceProxy(String address)
	{
		_address = address;
		Application.logInfo(SUB_LOG_TAG, "create instance: address='%s'", _address);
	}
	
	/**
	 * Авторизация на сервере
	 * @param login имя пользователя
	 * @param password пароль
	 * @return
	 */
	public SessionResponse Authorize(String login, String password) 
	{
		// входные параметры
		PropertyInfo[] params = new PropertyInfo[2];
		
        PropertyInfo pi = new PropertyInfo();
        pi.setName("login");
        pi.setValue(login);
        pi.setType(String.class.getClass());
        params[0] = pi;
        
        pi = new PropertyInfo();
        pi.setName("password");
        pi.setValue(password);
        pi.setType(String.class.getClass());
        params[1] = pi;
		
		SessionResponse resp = new SessionResponse();
		InvokeWebMethod("Authorize", params, TIMEOUT_SHORT, resp);
		return resp;
	}
	
	/**
	 * Закрытие сессии
	 * @param sessionId идентификатор сессии
	 * @return
	 */
	public SessionResponse Close(String sessionId) 
	{
		// входные параметры
		PropertyInfo[] params = new PropertyInfo[1];
		
        PropertyInfo pi = new PropertyInfo();
        pi.setName("sessionId");
        pi.setValue(sessionId);
        pi.setType(String.class.getClass());
        params[0] = pi;
        
		SessionResponse resp = new SessionResponse();
		InvokeWebMethod("Close", params, TIMEOUT_SHORT, resp);
		return resp;
	}

	/**
	 * Получение списка справочников
	 * @param sessionId идентификатор текущей сессии
	 * @return
	 */
	public DataResponse GetCatalogs(String sessionId) 
	{
		// входные параметры
		PropertyInfo[] params = new PropertyInfo[1];
		
        PropertyInfo pi = new PropertyInfo();
        pi.setName("sessionId");
        pi.setValue(sessionId);
        pi.setType(String.class.getClass());
        params[0] = pi;
        
        DataResponse resp = new DataResponse(DataItem.TYPE_CATALOG);
		InvokeWebMethod("GetCatalogs", params, TIMEOUT, resp);
		return resp;
	}

	/**
	 * Получение элементов справочника
	 * @param sessionId идентификатор текущей сессии
	 * @param catalogName имя справочника
	 * @param indexFrom
	 * @param indexTo
	 * @param parentId
	 * @return
	 */
	public DataResponse GetCatalogItems(
			String sessionId,
			String catalogName,
			int indexFrom, 
			int indexTo,
			String parentId) 
	{
		// входные параметры
		PropertyInfo[] params = new PropertyInfo[5];
		
        PropertyInfo pi = new PropertyInfo();
        pi.setName("sessionId");
        pi.setValue(sessionId);
        pi.setType(String.class.getClass());
        params[0] = pi;
        
        pi = new PropertyInfo();
        pi.setName("catalogName");
        pi.setValue(catalogName);
        pi.setType(String.class.getClass());
        params[1] = pi;
        
        pi = new PropertyInfo();
        pi.setName("indexFrom");
        pi.setValue(indexFrom);
        pi.setType(String.class.getClass());
        params[2] = pi;
        
        pi = new PropertyInfo();
        pi.setName("indexTo");
        pi.setValue(indexTo);
        pi.setType(String.class.getClass());
        params[3] = pi;
        
        pi = new PropertyInfo();
        pi.setName("parentId");
        pi.setValue(parentId);
        pi.setType(String.class.getClass());
        params[4] = pi;
        
        DataResponse resp = new DataResponse(DataItem.TYPE_CATALOG_ITEM);
		InvokeWebMethod("GetCatalogItems", params, TIMEOUT, resp);
		return resp;
	}
	
	/**
	 * Получить все элементы справочника
	 * @param sessionId идентификатор текущей сессии
	 * @param catalogName
	 * @return
	 */
	public DataResponse GetAllCatalogItems(String sessionId, String catalogName) 
	{
		// входные параметры
		PropertyInfo[] params = new PropertyInfo[2];
		
        PropertyInfo pi = new PropertyInfo();
        pi.setName("sessionId");
        pi.setValue(sessionId);
        pi.setType(String.class.getClass());
        params[0] = pi;
        
        pi = new PropertyInfo();
        pi.setName("catalogName");
        pi.setValue(catalogName);
        pi.setType(String.class.getClass());
        params[1] = pi;
        
        DataResponse resp = new DataResponse(DataItem.TYPE_CATALOG_ITEM);
		InvokeWebMethod("GetAllCatalogItems", params, TIMEOUT, resp);
		return resp;
	}
	
	/**
	 * Получить детальную информацию по элементу справочника
	 * @param sessionId идентификатор текущей сессии
	 * @param catalogName
	 * @param itemId
	 * @return
	 */
	public DataResponse GetCatalogItemDetails(String sessionId, String catalogName, String itemId)
	{
		// входные параметры
		PropertyInfo[] params = new PropertyInfo[3];
		
        PropertyInfo pi = new PropertyInfo();
        pi.setName("sessionId");
        pi.setValue(sessionId);
        pi.setType(String.class.getClass());
        params[0] = pi;
        
        pi = new PropertyInfo();
        pi.setName("catalogName");
        pi.setValue(catalogName);
        pi.setType(String.class.getClass());
        params[1] = pi;
        
        pi = new PropertyInfo();
        pi.setName("itemId");
        pi.setValue(itemId);
        pi.setType(String.class.getClass());
        params[2] = pi;
        
        DataResponse resp = new DataResponse(DataItem.TYPE_CATALOG_ITEM_PROPERTY);
		InvokeWebMethod("GetCatalogItemDetails", params, TIMEOUT, resp);
		return resp;
	}
	
	/**
	 * Получение списка документов
	 * @param sessionId идентификатор текущей сессии
	 * @return
	 */
	public DataResponse GetDocuments(String sessionId) 
	{
		// входные параметры
		PropertyInfo[] params = new PropertyInfo[1];
		
        PropertyInfo pi = new PropertyInfo();
        pi.setName("sessionId");
        pi.setValue(sessionId);
        pi.setType(String.class.getClass());
        params[0] = pi;
        
        DataResponse resp = new DataResponse(DataItem.TYPE_DOCUMENT);
		InvokeWebMethod("GetDocuments", params, TIMEOUT, resp);
		return resp;
	}
	
	/**
	 * Вызывает метод вб-сервиса
	 * @param <T> тип результата вызова
	 * @param methodName имя веб-метода
	 * @param params входные параметры веб-метода
	 * @param timeout таймаут
	 * @param resp результат вызова веб-метода
	 */
	private <T extends Response> void InvokeWebMethod(String methodName, PropertyInfo[] params, int timeout, T resp)
	{
		final String NAMESPACE = "http://www.exallon.ru";
		String SOAP_ACTION = "http://www.exallon.ru/DataService/" + methodName;
        
		// создаем запрос
        SoapObject request = new SoapObject(NAMESPACE, methodName);
        
        // добавляем в запрос входные параметры
        for (PropertyInfo pi : params)
        	request.addProperty(pi);
        
        // создаем soap-конверт
        SoapSerializationEnvelope envelope = new SoapSerializationEnvelope(SoapEnvelope.VER11);
        envelope.dotNet = true;
        envelope.setOutputSoapObject(request);
        
        HttpTransportSE transport = new HttpTransportSE(_address, timeout);
        
        // вызываем веб-сервис
        try
        {
            transport.call(SOAP_ACTION, envelope);
            SoapObject response = (SoapObject)envelope.getResponse();
            resp.deserialize(response);
        }
        catch(SocketTimeoutException ex)
        {
        	Application.logError(SUB_LOG_TAG, ex, "call [%s] failed: timeout", methodName);
        	resp.setError(ServerError.CLIENT_CONNECT_TIMEOUT);
        }
        catch (UnknownHostException ex)
        {
        	Application.logError(SUB_LOG_TAG, ex, "call [%s] failed: unknown host", methodName);
        	resp.setError(ServerError.CLIENT_UNKNOWN_HOST);
        }
        catch(Exception ex)
        {
        	Application.logError(SUB_LOG_TAG, ex, "call [%s] failed", methodName);
        	resp.setError(ServerError.CLIENT_UNKNOWN_ERROR);
        }
	}
}
