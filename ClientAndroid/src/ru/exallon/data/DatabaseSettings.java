package ru.exallon.data;

import java.util.UUID;
import org.w3c.dom.Element;

import ru.exallon.utils.XmlHelper;

/**
 * Настройка БД
 */
public class DatabaseSettings 
{
	/**
	 * Уникальный идентификатор данной настройки БД
	 */
	public UUID Id;
	/**
	 * Наименование БД
	 */
	public String Name;
	/**
	 * Адрес веб-сервиса, к которому нужно обращаться для получения информации
	 */
	public String ServerUrl;
	/**
	 * Признак "Выполнять автоматический вход" без запроса имени пользователя и пароля
	 */
	public boolean AutoConnect;
	/**
	 * Имя пользователя
	 */
	public String Username;
	/**
	 * Пароль
	 */
	public String Password;
	
	/**
	 * Закрытый конструктор
	 */
	private DatabaseSettings()
	{
	}
	
	/**
	 * Создает новый экземпляр
	 */
	public static DatabaseSettings New()
	{
		DatabaseSettings dbSettings = new DatabaseSettings();
		dbSettings.Id = UUID.randomUUID();
		dbSettings.ServerUrl = "";
		return dbSettings;
	}
	
	/**
	 * Загружает объект из xml-элемента
	 */
	public static DatabaseSettings loadFromXmlElement(Element elem)
	{
		DatabaseSettings dbSettings = new DatabaseSettings();
		dbSettings.Id = UUID.fromString(elem.getAttribute("Id"));
		dbSettings.Name = elem.getAttribute("Name");
		dbSettings.ServerUrl = elem.getAttribute("ServerUrl");
		dbSettings.AutoConnect = new Boolean(elem.getAttribute("AutoConnect"));
		dbSettings.Username = elem.getAttribute("Username");
		dbSettings.Password = elem.getAttribute("Password");
		
		return dbSettings;
	}
	
	public void saveToXmlElement(Element elem)
	{
		XmlHelper.addAttr(elem, "Id", Id.toString());
		XmlHelper.addAttr(elem, "Name", Name);
		XmlHelper.addAttr(elem, "ServerUrl", ServerUrl);
		XmlHelper.addAttr(elem, "AutoConnect", new Boolean(AutoConnect).toString());
		XmlHelper.addAttr(elem, "Username", Username);
		XmlHelper.addAttr(elem, "Password", Password);
	}
	
	@Override
	public String toString() 
	{
		return Name;
	}
	
	@Override
	public boolean equals(Object o) 
	{
		if (o == null || !(o instanceof DatabaseSettings))
			return false;
		
		return ((DatabaseSettings)o).Id.equals(this.Id);
	}
	
	/**
	 * Дополняет url сервера префиксом с именем протокола при необходимости
	 */
	public static String formatServerAddresss(String url)
	{
    	final String PREFIX = "http://";
    	return url.startsWith(PREFIX) ? url : PREFIX + url;
	}
}
