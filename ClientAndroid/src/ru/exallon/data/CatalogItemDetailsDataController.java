package ru.exallon.data;

import java.util.ArrayList;

import ru.exallon.server.DataResponse;
import ru.exallon.server.ServerError;

public class CatalogItemDetailsDataController extends DataController 
{
	private CacheManager _cacheManager;
	private String _catalogId;
	/**
	 * Идентификатор элемента справочника, детальную инф-цию которого получает контроллер
	 * Используется, как идентификатор кэша
	 */
	private String _itemId;
	private String _name;
	
	public CatalogItemDetailsDataController(int type, String catalogId, String itemId, String name) 
	{
		super(type);
		_cacheManager = CacheManager.getInstance();
		
		_catalogId = catalogId;
		_itemId = itemId;
		_name = name;
	}

	@Override
	protected Data loadDataFromCache(String filterText) 
	{
		return _cacheManager.getData(_itemId, filterText, DataItem.EMPTY_ID);
	}

	@Override
	protected void clearCache() 
	{
		_cacheManager.clear(_itemId);
	}

	@Override
	protected void addChankToCache(ArrayList<DataItem> chank, boolean lastChank) 
	{
		_cacheManager.addDataChank(_itemId, chank, lastChank);
	}

	@Override
	protected ArrayList<DataItem> loadChankFromServer(int indexFrom, int indexTo) 
	{
		// частичная загрузка данных не поддерживается
		return null;
	}

	@Override
	protected ArrayList<DataItem> loadAllDataFromServer() 
	{
		DataResponse resp = SessionManager.getInstance().getCatalogItemDetails(_catalogId, _itemId);
		
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

	@Override
	public DataControllerFactory selectDataItem(DataItem dataItem) 
	{
		// выбор реквизита элемента данных не обрабатываем
		return null;
	}

	@Override
	public String getTitle() 
	{
		return _name;
	}
}
