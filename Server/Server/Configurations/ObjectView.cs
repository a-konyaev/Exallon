using System;

namespace Exallon.Configurations
{
    /// <summary>
    /// Абстрактный описатель отображения объекта
    /// </summary>
    [Serializable]
    public abstract class ObjectView
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Наименование для отображения
        /// </summary>
        public string Presentation { get; set; }
        /// <summary>
        /// Отображать или нет объект
        /// </summary>
        public bool Show { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"> </param>
        /// /// <param name="presentation"> </param>
        /// <param name="show"></param>
        protected ObjectView(int id, string name, string presentation, bool show)
        {
            Id = id;
            Name = name;
            Presentation = presentation;
            Show = show;
        }
    }
}
