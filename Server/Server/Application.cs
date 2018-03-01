using System;
using System.Configuration;
using Exallon.Data;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Exallon
{
    /// <summary>
    /// Приложение
    /// </summary>
    public class Application : IDisposable
    {
        /// <summary>
        /// Экземпляр приложения
        /// </summary>
        public static Application Instance { get; private set; }
        /// <summary>
        /// IoC контейнер
        /// </summary>
        private IUnityContainer _container;
        /// <summary>
        /// Логгер
        /// </summary>
        public NLog.Logger Logger { get; private set; }
        /// <summary>
        /// Признак работы в режиме заглушки
        /// </summary>
        public bool StubMode { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public Application()
        {
            Instance = this;

            // подпишемся на обработку неотловленных исключений
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // создаем логгер
            CreateLogger();

            // читаем настройки приложения
            ReadAppSettings();

            // создаем контейнер
            CreateContainer();

            // инициализируем подсистемы
            InitSubsystems();

            // если работа в режиме заглушки
            if (StubMode)
                // то инициализируем заглушку
                DataService.InitStub();
        }

        /// <summary>
        /// Получить подсистему
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSubsystem<T>() where T : ISubsystem
        {
            return (T)_container.Resolve<ISubsystem>(typeof(T).Name);
        }

        /// <summary>
        /// Обработка всех неотловленных исключений
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var uex = (Exception)e.ExceptionObject;
            try
            {
                Logger.FatalException("Unhandled Exception!", uex);
            }
            catch (Exception)
            {
                Console.WriteLine("OnUnhandledException: " + uex);
            }

            try
            {
                Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnUnhandledException: dispose failed: " + ex);
            }

            Environment.Exit(1);
        }

        /// <summary>
        /// Создание логгера
        /// </summary>
        private void CreateLogger()
        {
            try
            {
                Logger = NLog.LogManager.GetCurrentClassLogger();
            }
            catch (Exception ex)
            {
                throw new Exception("CreateLogger failed", ex);
            }
        }

        /// <summary>
        /// Читаем настройки приложения
        /// </summary>
        private void ReadAppSettings()
        {
            try
            {
                var stubModeStr = ConfigurationManager.AppSettings["StubMode"];
                if (stubModeStr == null)
                    return;

                StubMode = bool.Parse(stubModeStr);
                if (StubMode)
                    Logger.Info("Work in STUB MODE!");
            }
            catch (Exception ex)
            {
                Logger.FatalException("Read application settings failed", ex);
                throw new Exception("ReadAppSettings failed", ex);
            }
        }

        /// <summary>
        /// Создание IoC-контейнера
        /// </summary>
        /// <returns></returns>
        private void CreateContainer()
        {
            Logger.Trace("Creating container...");

            try
            {
                var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
                _container = section.Configure(new UnityContainer());
            }
            catch (Exception ex)
            {
                Logger.FatalException("Create container failed", ex);
                throw new Exception("CreateContainer failed", ex);
            }
        }

        /// <summary>
        /// Инициализируем подсистемы
        /// </summary>
        private void InitSubsystems()
        {
            Logger.Trace("Init subsystems...");

            try
            {
                var subsystems = _container.ResolveAll<ISubsystem>();
                foreach (var subsystem in subsystems)
                {
                    try
                    {
                        subsystem.Init();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Init subsystem '{0}' failed: {1}", subsystem.GetType(), ex));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.FatalException("Init subsystems failed", ex);
                throw new Exception("InitSubsystems failed", ex);
            }
        }

        /// <summary>
        /// Освобождаем ресурсы
        /// </summary>
        public void Dispose()
        {
            Logger.Trace("Disposing...");

            try
            {
                var subsystems = _container.ResolveAll<ISubsystem>();

                foreach (var subsystem in subsystems)
                {
                    try
                    {
                        subsystem.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.WarnException(
                            string.Format("Dispose failed: dispose subsystem '{0}' error", subsystem.GetType()), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WarnException("Dispose failed", ex);
                return;
            }
        }
    }
}
