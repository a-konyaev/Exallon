using System;
using System.ServiceModel;

namespace Exallon
{
    /// <summary>
    /// Подсистема, которая включает в себя веб-сервис
    /// </summary>
    public abstract class ServiceSubsystem : Subsystem
    {
        /// <summary>
        /// Имя сервиса
        /// </summary>
        private readonly string _serviceName;
        /// <summary>
        /// Тип сервиса
        /// </summary>
        private readonly Type _serviceType;
        /// <summary>
        /// Хост сервиса
        /// </summary>
        private ServiceHost _serviceHost;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceType"></param>
        protected ServiceSubsystem(string serviceName, Type serviceType)
        {
            _serviceName = serviceName;
            _serviceType = serviceType;
        }

        /// <summary>
        /// Запустить сервис
        /// </summary>
        protected void StartService()
        {
            try
            {
                _serviceHost = new ServiceHost(_serviceType);
                _serviceHost.Open();

                Logger.Info("Service '{0}' started at address '{1}'", _serviceName, _serviceHost.BaseAddresses[0]);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Starting service '{0}' failed: {1}", _serviceName, ex.Message));
            }
        }

        /// <summary>
        /// Остановить сервис
        /// </summary>
        protected void StopService()
        {
            if (_serviceHost == null)
                return;
            
            try
            {
                _serviceHost.Close();
                _serviceHost = null;

                Logger.Info("Service '{0}' stopped", _serviceName);
            }
            catch (Exception ex)
            {
                Logger.Warn("Error of stopping service '{0}': {1}", _serviceName, ex.Message);
            }
        }
    }
}
