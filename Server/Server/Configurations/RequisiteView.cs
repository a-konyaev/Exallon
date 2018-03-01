using System;
using Enterra.V8x1C.DOM;

namespace Exallon.Configurations
{
    /// <summary>
    /// Описатель ототбражения реквизита
    /// </summary>
    [Serializable]
    public class RequisiteView : ObjectView
    {
        /// <summary>
        /// Идентификатор реквизита "Код"
        /// </summary>
        public const int REQUISITE_CODE_ID = -1;
        /// <summary>
        /// Идентификатор реквизита "Наименование"
        /// </summary>
        public const int REQUISITE_NAME_ID = -2;

        /// <summary>
        /// Информация о типе реквизите
        /// </summary>
        public TypeInfo TypeInfo { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="presentation"></param>
        /// <param name="show"> </param>
        /// <param name="typeInfo"></param>
        public RequisiteView(int id, string name, string presentation, bool show, TypeInfo typeInfo)
            : base(id, name, presentation, show)
        {
            TypeInfo = typeInfo;
        }
    }
}
