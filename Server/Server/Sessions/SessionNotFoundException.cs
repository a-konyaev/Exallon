namespace Exallon.Sessions
{
	/// <summary>
	/// Исключение "Сессия не найдена"
	/// </summary>
    public class SessionNotFoundException : SessionException
	{
        public SessionNotFoundException(string sessionId)
            : base(sessionId)
		{
		}
	}
}
