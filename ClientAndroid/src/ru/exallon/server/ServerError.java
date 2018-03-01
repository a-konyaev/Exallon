package ru.exallon.server;

/**
 * Ошибка, возникшая на стороне сервера или при обращении к серверу
 */
public class ServerError 
{
	// коды ошибок, возникших на стороне клиента при обращении к серверу
	/**
	 * Код ошибки "Соединение с сервером не установлено"
	 */
    public static final int CLIENT_NO_CONNECTION = -4;
	/**
	 * Код ошибки "Сервер не доступен"
	 */
    public static final int CLIENT_CONNECT_TIMEOUT = -3;
    /**
	 * Код ошибки "Сервер не найден"
	 */
    public static final int CLIENT_UNKNOWN_HOST = -2;
	/**
	 * Код ошибки "Ошибка при обращении к серверу"
	 */
    public static final int CLIENT_UNKNOWN_ERROR = -1;
	/**
	 * Код ошибки "Ошибок нет"
	 */
    public static final int NO_ERROR = 0;
    
    // коды ошибок, возникших на стороне сервера 
    /**
     * Код ошибки "Неизвестная ошибка"
     */
    public static final int SERVER_UNKNOWN_ERROR = 1;
    /**
     * Код ошибки "Лицензия истекла или отсутствует"
     */
    public static final int SERVER_LICENSE_EXPIRED = 2;
    /**
     * Код ошибки "Неправильный логин и/или пароль"
     */
    public static final int SERVER_UNAUTHORIZED = 3;
    /**
     * Код ошибки "Сессия не найдена"
     */
    public static final int SERVER_SESSION_NOT_FOUND = 4;
    
    /**
	 * Код ошибки
	 */
	private int _code;
	/**
	 * Описание ошибки
	 */
	private String _description;
	
	/**
	 * Код ошибки
	 */
	public int getCode()
	{
		return _code;
	}
	
	/**
	 * Описание ошибки
	 */
	public String getDescription()
	{
		return _description;
	}
	
	/**
	 * Это ошибка на клиенте
	 */
	public boolean isClientError()
	{
		return _code < 0;
	}
	
	/**
	 * Это ошибка на сервере
	 */
	public boolean isServerError()
	{
		return _code > 0;
	}
	
	/**
	 * Конструктор
	 * @param code код ошибки
	 * @param description описание ошибки
	 */
	public ServerError(int code, String description)
	{
		_code = code;
		_description = description;
	}
	
	/**
	 * Конструктор
	 * @param code код ошибки
	 */
	public ServerError(int code)
	{
		_code = code;
		
		// установим описание ошибки, которое будет отображаться пользователю
		switch (_code)
		{
		case CLIENT_NO_CONNECTION:
			_description = "Соединение с сервером не установлено";
			break;
		case CLIENT_CONNECT_TIMEOUT:
			_description = "Сервер не доступен";
			break;
		case CLIENT_UNKNOWN_HOST:
			_description = "Сервер не найден";
			break;
		case SERVER_LICENSE_EXPIRED:
			_description = "Лицензия истекла или отсутствует";
			break;
		case SERVER_UNAUTHORIZED:
			_description = "Неправильное имя пользователя или пароль";
			break;
		case SERVER_SESSION_NOT_FOUND:
			_description = "Сеанс работы завершён. Выполните вход повторно";
			break;

		case CLIENT_UNKNOWN_ERROR:	
		case SERVER_UNKNOWN_ERROR:
		default:
			_description = "Ошибка при обращении к серверу";
			break;
		}
	}
}
