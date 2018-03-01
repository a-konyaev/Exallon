package ru.exallon.data;

import java.util.ArrayList;

import ru.exallon.Application;
import ru.exallon.server.ServerError;

/**
 * Контроллер данных
 */
public abstract class DataController 
{
	private String SUB_LOG_TAG;
	
	/**
	 * Тип контроллера
	 */
	private int _type;
	/**
	 * Тип контроллера
	 */
	public int getType()
	{
		return _type;
	}
	/**
	 * Размер порции данных (кол-во элементов в 1 порции)
	 */
	private int _chankSize;
	/**
	 * Кол-во уже загруженных порций данных
	 * Например, если уже загружено 60 элементов, а размер порции = 20 элементов, 
	 * то всего загружено 3 порции. 
	 */
	private int _loadedChankCount;
	/**
	 * Ошибка, которая могла возникнуть при загрузке данных.
	 * Проверять наличие ошибки нужно перед вызовом метода getData
	 */
	private ServerError _serverError;
	
//########################################################################################
//ИНИЦИАЛИЗАЦИЯ
	
	/**
	 * Конструктор
	 */
	public DataController(int type)
	{
		String typeName = DataControllerFactory.getControllerTypeName(type);
		SUB_LOG_TAG = typeName + "DataController";
		
		_type = type;
		_chankSize = ConfigManager.getInstance().getCountOfElementsOnPage();
		_loadedChankCount = 0;
		_serverError = null;
		
		Application.logVerbose(SUB_LOG_TAG, "DataController created");
	}

//########################################################################################
//ЗАГРУЗКА ДАННЫХ
	
	public boolean hasError()
	{
		return _serverError != null;
	}
	
	public ServerError getError()
	{
		return _serverError;
	}
	
	protected void setError(ServerError error)
	{
		_serverError = error;
	}
	
	/**
	 * Возвращает данные, которые в данный момент загружены в контроллер
	 */
	public Data getData(String filterText) 
	{
		Application.logVerbose(SUB_LOG_TAG, "getData: filterText='%s'", filterText);
		
		// загружаем данные из кэша
		Data data = loadDataFromCache(filterText);
			
		// если кэш пуст
		if (data == null) 
		{
			// обновляем данные с сервера
			refresh();
			// и снова загружаем из кэша
			data = loadDataFromCache(filterText);
			// если кэш все также пуст
			if (data == null)
			{
				Application.logVerbose(SUB_LOG_TAG, "getData: no data");
				// значит данные так и не были загружены с сервера
				// (или на сервере данных нет, или при обращении к серверу произошла ошибка)
				return Data.getEmpty();
			}
		}
		
		return data;
	}
	
	/**
	 * Обновляет данные
	 */
	public void refresh()
	{
		Application.logVerbose(SUB_LOG_TAG, "refresh");

		// сбросим ошибку сервера
		_serverError = null;
		
		// загружаем все данные с сервера
		ArrayList<DataItem> chank = loadAllDataFromServer();
		
		// если в процессе загрузки данных с сервера произошла ошибка
		if (hasError() || chank == null)
		{
			Application.logVerbose(SUB_LOG_TAG, "refresh: error or no data");
			// то выходим
			return;
		}
		
		// очищаем кэш
		clearCache();
		
		_loadedChankCount = Integer.MAX_VALUE;
		
		// добавляем новую порцию данных в кэш
		addChankToCache(chank, true);
	}
	
	/**
	 * Загрузить следующую порцию данных с сервера
	 */
	public void loadNextChankFromServer() 
	{
		Application.logVerbose(SUB_LOG_TAG, "loadNextChankFromServer");
		
		// сбросим ошибку сервера
		_serverError = null;
		
		// вычислим индексы элементов данных очередной порции
		int indexFrom = _loadedChankCount * _chankSize;
		int indexTo = indexFrom +  _chankSize - 1;
		
		// загружаем порцию данных
		ArrayList<DataItem> chank = loadChankFromServer(indexFrom, indexTo);
		
		// если в процессе загрузки данных с сервера произошла ошибка
		if (hasError() || chank == null)
		{
			Application.logVerbose(SUB_LOG_TAG, "loadNextChankFromServer: error or no data");
			// то выходим
			return;
		}
		
		// увеличим кол-во загруженных порций
		_loadedChankCount++;
		
		// если кол-во загруженных элементов не равно размеру порции,
		// значит больше элементов на сервере нет, т.е. это была последняя порция
		//TODO: а что если на сервере кол-во элементов четно размеру порции?
		boolean lastChank = (chank.size() != _chankSize);
		
		// добавляем новую порцию данных в кэш
		addChankToCache(chank, lastChank);
	}
	

//########################################################################################
//Работа с кэшем
	
	/**
	 * Загрузка данных из кэша
	 * @return
	 */
	protected abstract Data loadDataFromCache(String filterText);
	
	/**
	 * Очистить кэш
	 */
	protected abstract void clearCache();
	
	/**
	 * Добавить порцию данных в кэш
	 * @param chank порция данных
	 * @param lastChank признак, что эта порция последняя, т.е. с сервера больше нечего загружать
	 */
	protected abstract void addChankToCache(ArrayList<DataItem> chank, boolean lastChank);
	
//########################################################################################
//Загрузка даннных с сервера
	
	/**
	 * Загрузить порцию данных с сервера
	 * @param indexFrom Индекс элемента, с которого должна начаться порция данных
	 * @param indexTo Индекс элемента, которым должна закончиться порция данных
	 * @return загруженная порция данных
	 */
	protected abstract ArrayList<DataItem> loadChankFromServer(int indexFrom, int indexTo);
	
	/**
	 * Загрузить все данные с сервера
	 * @return загруженные данные
	 */
	protected abstract ArrayList<DataItem> loadAllDataFromServer();
	

//########################################################################################
//Выбор элементов
	
	/**
	 * Обработка выбора элемента данных
	 * @param dataItem выбранный элемент данных
	 * @return фабрика дата-контроллера, к работе с данными которого нужно перейти
	 */
	public abstract DataControllerFactory selectDataItem(DataItem dataItem); 

//########################################################################################
//Внешний вид страницы

	/**
	 * Возвращает заголовок страницы
	 */
	public abstract String getTitle();
}
