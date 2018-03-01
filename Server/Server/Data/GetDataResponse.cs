using System.Runtime.Serialization;

namespace Exallon.Data
{
	/// <summary>
	/// Ответ на запрос получения данных
	/// </summary>
	[DataContract]
	public class GetDataResponse : Response
	{
		/// <summary>
		/// Массив данных
		/// </summary>
		[DataMember]
		public DataItem[] Data { get; private set; }
		
		/// <summary>
		/// Конструктор
		/// </summary>
        /// <param name="data">данные</param>
        public GetDataResponse(DataItem[] data)
		{
            Data = data;
		}

        /// <summary>
        /// Конструктор с указанием кода ошибки
        /// </summary>
        /// <param name="errorCode"></param>
        public GetDataResponse(int errorCode)
            : base(errorCode)
        {
            Data = new DataItem[0];
        }

        /// <summary>
        /// Конструктор с указанием кода и описания ошибки
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorDescription"></param>
        public GetDataResponse(int errorCode, string errorDescription)
            : base(errorCode, errorDescription)
        {
            Data = new DataItem[0];
        }
	}
}
