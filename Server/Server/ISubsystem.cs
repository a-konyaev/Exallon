using System;

namespace Exallon
{
    /// <summary>
    /// Интерфейс подсистемы
    /// </summary>
    public interface ISubsystem : IDisposable
    {
        /// <summary>
        /// Логгер подсистемы
        /// </summary>
        NLog.Logger Logger { get; }

        /// <summary>
        /// Инициализация
        /// </summary>
        void Init();
    }
}
