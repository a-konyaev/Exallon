package ru.exallon.data;

import java.util.ArrayList;

import ru.exallon.server.DataResponse;
import ru.exallon.server.ServerError;

public class DocumentsDataController extends DataController
{	

//########################################################################################
//Работа с кэшем
	
	/**
	 * Менеджер кэша
	 */
	private CacheManager _cacheManager;
	/**
	 * Идентификатор кэша "Документы"
	 */
    public static final String DOCUMENTS_CACHE_ID = "79E1F407-3B3B-4026-8492-DFD458064EDE";
	
	/**
	 * Конструктор
	 */
	public DocumentsDataController(int type)
	{
		super(type);
		_cacheManager = CacheManager.getInstance();
	}
	
	/**
	 * Загрузка данных из кэша
	 * @return
	 */
	@Override
	protected Data loadDataFromCache(String filterText)
	{
		return _cacheManager.getData(DOCUMENTS_CACHE_ID, filterText, null);
	}
	
	/**
	 * Очистить кэш
	 */
	@Override
	protected void clearCache()
	{
		_cacheManager.clear(DOCUMENTS_CACHE_ID);
	}
	
	/**
	 * Добавить порцию данных в кэш
	 * @param chank порция данных
	 * @param lastChank признак, что эта порция последняя, т.е. с сервера больше нечего загружать
	 */
	@Override
	protected void addChankToCache(ArrayList<DataItem> chank, boolean lastChank)
	{
		_cacheManager.addDataChank(DOCUMENTS_CACHE_ID, chank, lastChank);
	}
	
//########################################################################################
//Загрузка даннных с сервера
	
	/**
	 * Загрузить порцию данных с сервера
	 * @param indexFrom Индекс элемента, с которого должна начаться порция данных
	 * @param indexTo Индекс элемента, которым должна закончиться порция данных
	 * @return загруженная порция данных
	 */
	@Override
	protected ArrayList<DataItem> loadChankFromServer(int indexFrom, int indexTo)
	{
		return loadAllDataFromServer();
	}
	
	/**
	 * Загрузить все данные с сервера
	 * @return загруженные данные
	 */
	@Override
	protected ArrayList<DataItem> loadAllDataFromServer()
	{
		// получим полный список документов (диапазон не передаем)
		DataResponse resp = SessionManager.getInstance().getDocuments();
		
		// если возникла ошибка
		ServerError error = resp.getError();
		if (error != null)
		{
			// то запомним ее и выходим
			setError(error);
			return null;
		}
		
		return resp.getData();
	}
	
//########################################################################################
//Выбор элементов
		
	/**
	 * Обработка выбора элемента данных
	 * @param dataItem выбранный элемент данных
	 * @return фабрика дата-контроллера, к работе с данными которого нужно перейти
	 */
	@Override
	public DataControllerFactory selectDataItem(DataItem dataItem)
	{
		//TODO
		return null;
//		return DataControllerFactory.getCatalogItemsDCF(
//				dataItem.getId(), dataItem.getName(), null);
	}
	
//########################################################################################
//Внешний вид страницы
		
	/**
	 * Возвращает заголовок страницы
	 */
	@Override
	public String getTitle()
	{
		return "Документы";
	}
}
