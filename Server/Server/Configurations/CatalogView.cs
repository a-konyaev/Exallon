using System;
using System.Linq;
using System.Collections.Generic;

namespace Exallon.Configurations
{
    /// <summary>
    /// Описатель отображения справочника
    /// </summary>
    [Serializable]
    public class CatalogView : ObjectView
    {
        /// <summary>
        /// Реквизиты справочника
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
        public CatalogView(string name, string presentation, bool show, List<RequisiteView> requisites)
            : base(0, name, presentation, show)
        {
            Requisites = requisites;
        }
    }
}
