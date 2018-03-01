using System;
using System.Collections.Generic;
using System.Linq;

namespace Exallon.Configurations
{
    /// <summary>
    /// Описатель отображения документа
    /// </summary>
    [Serializable]
    public class DocumentView : ObjectView
    {
        /// <summary>
        /// Реквизиты документа
        /// </summary>
        public List<RequisiteView> Requisites { get; set; }

        /// <summary>
        /// Есть ли среди реквизитов реквизит "Код"
        /// </summary>
        public bool HasCodeRequisite
        {
            get { return Requisites.Count(r => r.Id == RequisiteView.REQUISITE_CODE_ID) == 1; }
        }

        /// <summary>
        /// Есть ли среди реквизитов реквизит "Наименование"
        /// </summary>
        public bool HasNameRequisite
        {
            get { return Requisites.Count(r => r.Id == RequisiteView.REQUISITE_NAME_ID) == 1; }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name"></param>
        /// <param name="presentation"></param>
        /// <param name="show"></param>
        /// <param name="requisites"></param>
        public DocumentView(string name, string presentation, bool show, List<RequisiteView> requisites)
            : base(0, name, presentation, show)
        {
            Requisites = requisites;
        }
    }
}
