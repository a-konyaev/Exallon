using System;
using System.Xml;

namespace Exallon.Utils
{
    /// <summary>
    /// Расширения для работы с Xml
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Добавить дочерний xml-элемент
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XmlElement AddChildElement(this XmlElement element, string name)
        {
            if (element.OwnerDocument == null)
                throw new InvalidOperationException("element has no owner document");

            var child = element.OwnerDocument.CreateElement(name);
            element.AppendChild(child);

            return child;
        }

        /// <summary>
        /// Добавить атрибут к xml-элементу
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static XmlElement AddAttribute(this XmlElement element, string name, string value)
        {
            if (element.OwnerDocument == null)
                throw new InvalidOperationException("element has no owner document");

            var att = element.OwnerDocument.CreateAttribute(name);
            att.Value = value;
            element.Attributes.Append(att);
            return element;
        }
    }
}
