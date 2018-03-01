package ru.exallon.utils;

import org.w3c.dom.Attr;
import org.w3c.dom.Element;

public class XmlHelper 
{
	/**
	 * Добавляет атрибут к xml-элементу
	 * @param elem
	 * @param name
	 * @param value
	 */
	public static void addAttr(Element elem, String name, String value)
	{
		Attr attr = elem.getOwnerDocument().createAttribute(name);
		attr.setValue(value);
		elem.setAttributeNode(attr);
	}
}
