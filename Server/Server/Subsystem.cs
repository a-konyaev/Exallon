using System;

namespace Exallon
{
    /// <summary>
    /// Базовый класс для всех подсистем
    /// </summary>
    public abstract class Subsystem : ISubsystem
    {
        /// <summary>
        /// Логгер подсистемы
        /// </summary>
        public NLog.Logger Logger { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        protected Subsystem()
        {
            try
            {
                Logger = NLog.LogManager.GetLogger(this.GetType().Name);
            }
            catch (Exception ex)
            {
                throw new Exception("Get logger failed", ex);
            }
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
