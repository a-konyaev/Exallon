using System;

namespace Exallon.Sessions
{
    /// <summary>
    /// Класс, которые хранит время обращения к объекту
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AccessTimeHolder<T>
    {
        /// <summary>
        /// Время последнего обращения к объекту
        /// </summary>
        public DateTime LastAccessTime { get; private set; }

        private readonly T _object;
        /// <summary>
        /// Объект
        /// </summary>
        public T Object
        {
            get
            {
                LastAccessTime = DateTime.Now;
                return _object;
            }
        }

        public AccessTimeHolder(T @object)
        {
            LastAccessTime = DateTime.Now;
            _object = @object;
        }
    }
}
