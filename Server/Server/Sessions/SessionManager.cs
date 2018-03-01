using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Enterra.V8x1C.DOM;
using Enterra.V8x1C.Routines;

namespace Exallon.Sessions
{
    /// <summary>
    /// Менеджер сессий работы с 1С
    /// </summary>
    public class SessionManager : Subsystem, ISessionManager
    {
        /// <summary>
        /// Объект для синхронизации доступа к словарю сессий
        /// </summary>
        private static readonly object s_sessionsSync = new object();

        /// <summary>
        /// Активные сессии: [Идентификатор сессии; сессия с временем последнего обращения к ней]
        /// </summary>
        private readonly Dictionary<String, AccessTimeHolder<Session>> _sessions = 
            new Dictionary<String, AccessTimeHolder<Session>>();

        #region Properties

        private string _connStr;
        private ConnectionSettings _connSettings;

        /// <summary>
        /// Строка подключения к БД 1С
        /// </summary>
        public string ConnectionString
        {

            get { return _connStr; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                // парсим строку подключения
                string dbPath;
                string serverAddress;
                string dbName;
                ParseConnString(value, out dbPath, out serverAddress, out dbName);

                if (dbPath != null)
                {
                    _connSettings = new ConnectionSettings
                                        {
                                            ConnectionType = ConnectionType.File,
                                            File = dbPath,
                                        };
                }
                else
                {
                    _connSettings = new ConnectionSettings
                                        {
                                            ConnectionType = ConnectionType.Server,
                                            Server = serverAddress,
                                            Base = dbName
                                        };
                }

                _connStr = value;
            }
        }

        /// <summary>
        /// Разбор строки подключения
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dbPath"></param>
        /// <param name="serverAddress"></param>
        /// <param name="dbName"></param>
        private static void ParseConnString(
            string input,
            out string dbPath,
            out string serverAddress,
            out string dbName)
        {
            if (input.StartsWith("DbPath="))
            {
                dbPath = input.Substring(7);
                if (!Directory.Exists(dbPath))
                    throw new ArgumentException("Folder not found: " + dbPath);

                serverAddress = null;
                dbName = null;
            }
            else if (input.StartsWith("Server=") && input.Contains(";DbName="))
            {
                dbPath = null;
                serverAddress = input.Substring(7, input.IndexOf(';') - 7);
                dbName = input.Substring(serverAddress.Length + 15);
            }
            else
            {
                throw new Exception("Wrong connection string format: " + input);
            }
        }

        private V8Version _v8Version;

        /// <summary>
        /// Версия 1С
        /// </summary>
        public string Version1C
        {
            get { return _v8Version.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                try
                {
                    _v8Version = (V8Version) Enum.Parse(typeof (V8Version), value, true);
                }
                catch
                {
                    throw new ArgumentException("Некорректное значение Version1C: " + value);
                }
            }
        }

        /// <summary>
        /// Период мониторинга сессий
        /// </summary>
        public TimeSpan SessionMonitoringPeriod { get; set; }

        /// <summary>
        /// Максимальное время простоя сессии
        /// </summary>
        public TimeSpan SessionMaxIdleTime { get; set; }

        #endregion

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void Init()
        {
            StartSessionMonitoring();
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public override void Dispose()
        {
            StopSessionMonitoring();
        }

        #region Мониторинг сессий

        /// <summary>
        /// Таймер мониторинга сессий
        /// </summary>
        private Timer _monitoringTimer;

        /// <summary>
        /// Запустить мониторинг сессий
        /// </summary>
        private void StartSessionMonitoring()
        {
            _monitoringTimer = new Timer(Monitoring, null, SessionMonitoringPeriod, SessionMonitoringPeriod);
            Logger.Trace("Session monitoring started");
        }

        /// <summary>
        /// Остановить мониторинг сессий
        /// </summary>
        private void StopSessionMonitoring()
        {
            if (_monitoringTimer == null)
                return;

            _monitoringTimer.Dispose();
            _monitoringTimer = null;
            Logger.Trace("Session monitoring stopped");
        }

        private void Monitoring(object state)
        {
            Logger.Trace("Session monitoring run...");
            var now = DateTime.Now;

            lock (s_sessionsSync)
            {
                // получим идентификаторы просроченных сессий
                var expiredSessionKeys = _sessions.Where(entry => now - entry.Value.LastAccessTime > SessionMaxIdleTime)
                    .Select(entry => entry.Key).ToArray();

                // закроем и удалим просроченные сессии
                foreach (var sessionId in expiredSessionKeys)
                {
                    _sessions.Remove(sessionId);
                    Logger.Info("Monitoring: session '{0}' has been closed automatically", sessionId);
                }
            }

            GC.Collect();
        }

        #endregion

        #region ISessionManager

        /// <summary>
        /// Создать (открыть) новую сессию
        /// </summary>
        /// <param name="login">имя пользователя</param>
        /// <param name="password">пароль пользователя</param>
        /// <returns>идентификатор созданной сессии</returns>
        public string CreateNewSession(string login, string password)
        {
            _connSettings.UserName = login;
            _connSettings.Password = password;

            Session session;
            try
            {
                session = Application.Instance.StubMode 
                    ? Session.Empty
                    : new Session(_v8Version, _connSettings);
            }
            catch (Exception ex)
            {
                Logger.Error("CreateNewSession failed: " + ex.Message);
                throw;
            }

            var sessionId = session.Id;
            
            lock (s_sessionsSync)
            {
            	_sessions.Add(sessionId, new AccessTimeHolder<Session>(session));
            }

            Logger.Info("CreateNewSession: session '{0}' created", sessionId);
            return sessionId;
        }

        /// <summary>
        /// Закрыть сессию
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        public void CloseSession(string sessionId)
        {
            lock (s_sessionsSync)
            {
                // если такой сессии нет
                if (!_sessions.ContainsKey(sessionId))
                {
                    // залогируем предупреждение
                    Logger.Warn("CloseSession: session '{0}' not found", sessionId);
                    return;
                }

                _sessions.Remove(sessionId);
            }

            GC.Collect();
            Logger.Info("CloseSession: session '{0}' has been closed", sessionId);
        }

        /// <summary>
        /// Получить сессию с заданным идентификатором
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <returns></returns>
        public Session GetSession(string sessionId)
        {
            return GetSession(sessionId, false);
        }

        /// <summary>
        /// Получить сессию с заданным идентификатором
        /// </summary>
        /// <param name="sessionId">идентификатор сессии</param>
        /// <param name="recreate">нужно ли создать сессию заново, а текущую открытую с таким же идентификатором закрыть</param>
        /// <returns></returns>
        private Session GetSession(string sessionId, bool recreate)
        {
            string connStr;

        	lock (s_sessionsSync)
        	{
                if (!_sessions.ContainsKey(sessionId))
                {
                    Logger.Error("GetSession: session '{0}' not found", sessionId);
                    throw new SessionNotFoundException(sessionId);
                }

        	    var currentSession = _sessions[sessionId].Object;

                // если пересоздавать сессию не нужно
        	    if (!recreate)
        	        return currentSession;

                connStr = currentSession.ConnectionString;
        	    _sessions.Remove(sessionId);
        	}

            GC.Collect();
            var newSession = Application.Instance.StubMode ? null : new Session(_v8Version, connStr);

            lock (s_sessionsSync)
            {
            	_sessions.Add(sessionId, new AccessTimeHolder<Session>(newSession));
            }

            Logger.Info("GetSession: session '{0}' has been recreated", sessionId);
            return newSession;
        }

        /// <summary>
        /// Выполнить операцию в контексте сессии
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="func"></param>
        public T ExecuteWithinSession<T>(string sessionId, Func<Session, T> func)
        {
            // максимальное кол-во попыток выполнить операцию
            const int maxTryCount = 3;

            var tryCount = 0;
            while (true)
            {
                var session = GetSession(sessionId, tryCount > 0);
                
                try
                {
                    return func(session);
                }
                catch
                {
                    if (++tryCount < maxTryCount)
                        continue;

                    throw;
                }
            }
        }

        #endregion
    }
}
