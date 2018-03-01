namespace Exallon.Data
{
    /// <summary>
    /// Менеджер данных
    /// </summary>
    public class DataManager : ServiceSubsystem, IDataManager
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public DataManager()
            : base("DataService", typeof(DataService))
        {
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void Init()
        {
            StartService();
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public override void Dispose()
        {
            StopService();
        }
    }
}
