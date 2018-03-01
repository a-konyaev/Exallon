package ru.exallon.data;

import java.io.File;
import java.io.FileOutputStream;
import java.util.ArrayList;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import android.content.Context;
import ru.exallon.Application;
import ru.exallon.utils.XmlHelper;

public class ConfigManager 
{
	private static String SUB_LOG_TAG = "ConfigManager";
	private final String CONFIG_FILE = "config.xml";
	
	private static ConfigManager _instance;
	
	/**
	 * Экземпляр класса
	 */
	public static synchronized ConfigManager getInstance()
	{
		if (_instance == null)
			_instance = new ConfigManager();
		
		return _instance;
	}
	
	/**
	 * Закрытый конструктор
	 */
	private ConfigManager()
	{
		load();
	}
	
	/**
	 * Список настроек БД
	 */
	private ArrayList<DatabaseSettings> _dbSettingsList;
	
	/**
	 * Список настроек БД
	 */
	public ArrayList<DatabaseSettings> getDatabaseSettings()
	{
		return _dbSettingsList;
	}
	
	/**
	 * Кол-во элементов на 1 странице 
	 */
	private int _countOfElementsOnPage;
	
	/**
	 * Кол-во элементов на 1 странице 
	 */
	public int getCountOfElementsOnPage()
	{
		return _countOfElementsOnPage;
	}
	
	public void setCountOfElementsOnPage(int value)
	{
		_countOfElementsOnPage = value;
	}
	
	/**
	 * Загрузка данных из постоянного хранилища
	 */
	public void load()
	{
		// инициализация настроек по умолчанию
		_dbSettingsList = new ArrayList<DatabaseSettings>();
		_countOfElementsOnPage = 100;
		
		Context context = Application.getContext();
		File file = context.getFileStreamPath(CONFIG_FILE);
		
		if (!file.exists())
		{
			Application.logInfo(SUB_LOG_TAG, "load: config file does not exist");
			return;
		}
			
		Document doc;
		try 
		{
			DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
			DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
			doc = dBuilder.parse(file);
			
			loadCountOfElementsOnPage(doc);
			loadDatabaseSettings(doc);
		} 
		catch (Exception ex) 
		{
			Application.logError(SUB_LOG_TAG, ex, "load failed");
			return;
		}
	}
	
	private void loadCountOfElementsOnPage(Document doc) throws Exception
	{
		try
		{
			NodeList nodes = doc.getElementsByTagName("CountOfElementsOnPage");		
			Element elem = (Element)nodes.item(0);
			_countOfElementsOnPage = new Integer(elem.getAttribute("Value"));
		}
		catch (Exception ex)
		{
			Application.logError(SUB_LOG_TAG, ex, "loadCountOfElementsOnPage failed");
			throw new Exception("Ошибка загрузки параметра 'CountOfElementsOnPage'", ex);
		}
	}
	
	private void loadDatabaseSettings(Document doc) throws Exception
	{
		try
		{
			NodeList nodes = doc.getElementsByTagName("Database");
			for (int i = 0; i < nodes.getLength(); i++) 
			{
				Node node = nodes.item(i);
				if(node.getNodeType() != Node.ELEMENT_NODE)
					continue;
				
				DatabaseSettings dbSettings = DatabaseSettings.loadFromXmlElement((Element)node);
				_dbSettingsList.add(dbSettings);
			}
		}
		catch (Exception ex)
		{
			Application.logError(SUB_LOG_TAG, ex, "loadDatabaseSettings failed");
			throw new Exception("Ошибка загрузки 'DatabaseSettings'", ex);
		}
	}
	
	/**
	 * Сохранение данных в постоянное хранилище
	 */
	public void save()
	{
		Context context = Application.getContext();
	
		try 
		{
			DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
			DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
			
			Document doc = dBuilder.newDocument();
			
			Element root = doc.createElement("Config");
			doc.appendChild(root);
			
			saveCountOfElementsOnPage(doc, root);
			saveDatabaseSettings(doc, root);
			
			TransformerFactory transformerFactory = TransformerFactory.newInstance();
			Transformer transformer = transformerFactory.newTransformer();
			
			DOMSource source = new DOMSource(doc);
			
			FileOutputStream fos = context.openFileOutput(CONFIG_FILE, Context.MODE_PRIVATE);
			StreamResult result = new StreamResult(fos);
			
			transformer.transform(source, result);
		} 
		catch (Exception ex) 
		{
			Application.logError(SUB_LOG_TAG, ex, "save failed");
			return;
		}
	}

	private void saveCountOfElementsOnPage(Document doc, Element root)
	{
		Element elem = doc.createElement("CountOfElementsOnPage");
		root.appendChild(elem);
		XmlHelper.addAttr(elem, "Value", Integer.toString(_countOfElementsOnPage));
	}
	
	private void saveDatabaseSettings(Document doc, Element root)
	{
		Element localRoot = doc.createElement("DatabaseSettings");
		root.appendChild(localRoot);
		
		for (DatabaseSettings item : _dbSettingsList) 
		{
			Element elem = doc.createElement("Database");
			localRoot.appendChild(elem);
			item.saveToXmlElement(elem);
		}
	}
	
	/**
	 * Проверяет, есть ли уже другая настройка БД с таким же именем 
	 * @param name наименование БД
	 * @param index индекс настройки БД, имя которой проверяется
	 * @return
	 */
	public boolean nameAlreadyExists(String name, int index) {
		for (int i = 0; i < _dbSettingsList.size(); i++) {
			if (i == index)
				continue;
			DatabaseSettings dbs = _dbSettingsList.get(i);
			if (dbs.Name.equals(name))
				return true;
		}
		return false;
	}
}
