package ru.exallon.data;

import java.util.ArrayList;


/**
 * Данные
 */
public class Data 
{
	/**
	 * Элементы данных
	 */
	private ArrayList<DataItem> _items;
	public ArrayList<DataItem> getItems()
	{
		return _items;
	}
	
	/**
	 * Признак того, что все данные, которые можно было загрузить с сервера, загружены
	 */
	private boolean _allDataLoaded;
	public boolean getAllDataLoaded()
	{
		return _allDataLoaded;
	}
	public void setAllDataLoaded(boolean allDataLoaded)
	{
		_allDataLoaded = allDataLoaded;
	}
	
	/**
	 * Конструктор
	 */
	public Data()
	{
		_items = new ArrayList<DataItem>();
		_allDataLoaded = false;
	}
	
	/**
	 * Пустый данные
	 * @return
	 */
	public static Data getEmpty()
	{
		Data data = new Data();
		data.setAllDataLoaded(true);
		return data;
	}
}
