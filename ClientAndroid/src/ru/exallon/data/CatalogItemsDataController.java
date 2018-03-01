package ru.exallon.data;

import java.util.ArrayList;

import ru.exallon.server.DataResponse;
import ru.exallon.server.ServerError;

public class CatalogItemsDataController extends DataController
{	
	/**
	 * Менеджер кэша
	 */
	private CacheManager _cacheManager;
	private String _catalogId;
	private String _catalogName;
	private String _parentItemId;
	
	/**
	 * Идентификатор справочника, элементами которого управляет данных контроллер
	 */
	public String getCatalogId()
	{
		return _catalogId;
	}
	
	public CatalogItemsDataController(
			int type, String catalogId, String catalogName, String parentItemId)
	{
		super(type);
		_cacheManager = CacheManager.getInstance();
		_catalogId = catalogId;
		_catalogName = catalogName;
		_parentItemId = parentItemId;
	}
	
//########################################################################################
//Работа с кэшем
	
	/**
	 * Загрузка данных из кэша
	 * @return
	 */
	@Override
	protected Data loadDataFromCache(String filterText)
	{
		// если строка поиска не задана, то передаем ИД родит. эл-та,
		// иначе не передаем, чтобы поиск выполнялся по всем элементам справочника
		String parentId = (filterText == null 
				? (_parentItemId == null ? DataItem.EMPTY_ID : _parentItemId)
				: null);
		
		return _cacheManager.getData(_catalogId, filterText, parentId);
	}
	
	/**
	 * Очистить кэш
	 */
	@Override
	protected void clearCache()
	{
		_cacheManager.clear(_catalogId);
	}
	
	/**
	 * Добавить порцию данных в кэш
	 * @param chank порция данных
	 * @param lastChank признак, что эта порция последняя, т.е. с сервера больше нечего загружать
	 */
	@Override
	protected void addChankToCache(ArrayList<DataItem> chank, boolean lastChank)
	{
		_cacheManager.addDataChank(_catalogId, chank, lastChank);
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
		DataResponse resp = SessionManager.getInstance().getCatalogItems(
				_catalogId, indexFrom, indexTo, _parentItemId);
		
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
	
	/**
	 * Загрузить все данные с сервера
	 * @return загруженные данные
	 */
	@Override
	protected ArrayList<DataItem> loadAllDataFromServer()
	{
		DataResponse resp = SessionManager.getInstance().getAllCatalogItems(_catalogId);
		
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
		return dataItem.getIsGroup()
			? DataControllerFactory.getCatalogItemsDCF(
					_catalogId, ".../" + dataItem.getName(), dataItem.getId())
			: DataControllerFactory.getCatalogItemDetailsDCF(
					_catalogId, dataItem.getId(), dataItem.getName());
	}

//########################################################################################
//Внешний вид страницы
		
	/**
	 * Возвращает заголовок страницы
	 */
	@Override
	public String getTitle()
	{
		return _catalogName;
	}
}
