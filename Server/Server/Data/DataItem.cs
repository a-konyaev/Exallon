using System.Runtime.Serialization;

namespace Exallon.Data
{
    /// <summary>
	/// Элемент данных
	/// </summary>
	[DataContract]
	public class DataItem
	{
        /// <summary>
        /// Пустой идентификатор
        /// </summary>
        public const string EMPTY_ID = "00000000-0000-0000-0000-000000000000";

		/// <summary>
		/// Идентификатор элемента данных
		/// </summary>
		[DataMember]
		public string Id { get; private set; }
        /// <summary>
        /// Наименование
        /// </summary>
        [DataMember]
        public string Name { get; private set; }
        /// <summary>
        /// Значение
        /// </summary>
        [DataMember]
        public string Value { get; private set; }
        /// <summary>
        /// Идентификатор родительского элемента
        /// </summary>
        [DataMember]
        public string ParentId { get; private set; }
        /// <summary>
        /// Является ли данный элемент группой, т.е. имеет дочерние элементы
        /// </summary>
        [DataMember]
        public bool IsGroup { get; private set; }
        

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="id">идентификатор</param>
        /// <param name="name">наименование</param>
        /// <param name="value">значение</param>
        public DataItem(string id, string name, string value)
            : this(id, name, value, EMPTY_ID, false)
		{
		}

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">идентификатор</param>
        /// <param name="name">наименование</param>
        /// <param name="value">значение</param>
        /// <param name="parentId">идентификатор родительского элемента</param>
        /// <param name="isGroup">является группой</param>
        public DataItem(string id, string name, string value, string parentId, bool isGroup)
        {
            Id = id;
            Name = name;
            Value = value;
            ParentId = parentId;
            IsGroup = isGroup;
        }
	}
}
