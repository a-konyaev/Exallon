package ru.exallon.server;

/**
 * Ответ, содержащий информацию о сессии
 */
public class SessionResponse extends Response
{
	private String _sessionId;
	/**
	 * Идентификатор сессии
	 */
	public String getSessionId()
	{
		return _sessionId;
	}
	
	public SessionResponse() {}
	
	public SessionResponse(String sessionId) 
	{
		_sessionId = sessionId;
	}
	
	@Override
	public void deserializeProperty(int index, Object value) throws Exception
	{
		if (index != 0)
			throw new Exception("Неожиданный индекс св-ва");
		
		_sessionId = (value == null ? null : value.toString());
	}
}