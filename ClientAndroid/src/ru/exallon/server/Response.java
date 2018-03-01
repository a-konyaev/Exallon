package ru.exallon.server;

import org.ksoap2.serialization.SoapObject;

import ru.exallon.Application;

/**
 * Базовый класс для всех ответов, которые возвращают методы веб-сервиса
 */
public abstract class Response 
{
	private static String SUB_LOG_TAG = "Response";
	
	/**
	 * Ошибка, возникшая на стороне сервера или при обращении к серверу
	 */
	private ServerError _error;
	
	public Response(){}
		
	/**
	 * Возвращает ошибку, которая может быть в отчете, если
	 * при выполнении операции произошла ошибка 
	 * @return
	 */
	public ServerError getError()
	{
		return _error;
	}
	
	/**
	 * Содержит ли ответ ошибку
	 */
	public boolean hasError()
	{
		return _error != null;
	}
	
	/**
	 * Установить в ответ ошибку
	 * @param errorCode код ошибки
	 * @param errorDescription описание ошибки
	 */
	public void setError(int errorCode, String errorDescription)
	{
		_error = new ServerError(errorCode, errorDescription);
	}
	
	/**
	 * Установить в ответ ошибку
	 * @param errorCode код ошибки
	 */
	public void setError(int errorCode)
	{
		_error = new ServerError(errorCode);
	}
	
	/**
	 * Десериализация св-ва в классе-наследнике
	 * @param index индекс св-ва: для класса-наследника начинается с 0
	 * @param value значение св-ва
	 * @throws Exception 
	 */
	public abstract void deserializeProperty(int index, Object value) throws Exception;
	
	/**
	 * Десериализация из soap-объекта
	 * @param so soap-объект
	 * @throws Exception исключение при ошибке десериализации
	 */
	public void deserialize(SoapObject so) throws Exception
	{
		int propsCount = so.getPropertyCount();
		
		if (propsCount < 2)
			throw new Exception("кол-во св-в меньше двух");
		
		// загрузим базовые св-ва
		Object value = so.getProperty(0);
		int errorCode = (value == null ? ServerError.NO_ERROR : Integer.parseInt(value.toString()));
		
		// если ответ содержит ошибку
		if (errorCode != ServerError.NO_ERROR)
		{
			value = so.getProperty(1);
			if (value == null)
				setError(errorCode);
			else
				setError(errorCode, value.toString());
		}
		
		// загрузим св-ва класса-наследника
		for (int i = 2; i < propsCount; i++)
        {
			try
			{
				deserializeProperty(i - 2, so.getProperty(i));
			}
        	catch (Exception ex)
        	{
        		Application.logError(SUB_LOG_TAG, ex, "deserialize failed: index=%d", i);
        		throw new Exception("Ошибка при десериализации св-ва index=" + i, ex);
        	}
        }
	}
}
