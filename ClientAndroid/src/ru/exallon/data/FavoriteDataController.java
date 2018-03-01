package ru.exallon.data;

import java.util.ArrayList;
import java.util.Arrays;

public class FavoriteDataController extends DataController 
{
	/**
	 * Менеджер кэша
	 */
	private CacheManager _cacheManager;
	/**
	 * Идентификатор кэша "Избранное"
	 */
    public static final String FAVORITE_CACHE_ID = "7817CA42-282A-4E9A-843B-172231D2DC5F";
    
    /**
	 * Конструктор
	 */
	public FavoriteDataController(int type)
	{
		super(type);
		_cacheManager = CacheManager.getInstance();
	}
	
	/**
	 * Добавить элемент в список Избранное
	 * @param dataItem элемент, который нужно добавить в Избранное
	 * @param dataId идентификатор списка данных, к которому относится элемент 
	 * (например, идентификатор соотв. справочника)
	 */
	public static void addDataItemToFavorite(DataItem dataItem, String dataId)
	{
		CacheManager cm = CacheManager.getInstance();
		
		DataItem exist = cm.findDataItem(FAVORITE_CACHE_ID, dataItem.getId());
		
		// если такой элемент уже есть в Избранных
		if (exist != null)
			// то повторно его не добавляем
			return;
		
		// создадим элемент для добавления, в котором в поле ParentId 
		// специально пропишем идентификатор списка данных
		DataItem forAdd = new DataItem(
				dataItem.getId(),
				dataItem.getType(),
				dataItem.getName(),
				dataItem.getValue(),
				dataId,
				dataItem.getIsGroup());
		cm.addDataChank(FAVORITE_CACHE_ID, new ArrayList<DataItem>(Arrays.asList(forAdd)), false);
	}
	
	/**
	 * Удалить элемент из списка Избранное
	 * @param item
	 */
	public static void removeDataItemFromFavorite(DataItem dataItem)
	{
		CacheManager.getInstance().removeDataItem(FAVORITE_CACHE_ID, dataItem);
	}
	
	@Override
	protected Data loadDataFromCache(String filterText) 
	{
		return _cacheManager.getData(FAVORITE_CACHE_ID, filterText, null);
	}

	@Override
	protected void clearCache() 
	{
		_cacheManager.clear(FAVORITE_CACHE_ID);
	}

	@Override
	protected void addChankToCache(ArrayList<DataItem> chank, boolean lastChank) 
	{
		_cacheManager.addDataChank(FAVORITE_CACHE_ID, chank, lastChank);
	}

	@Override
	protected ArrayList<DataItem> loadChankFromServer(int indexFrom, int indexTo) 
	{
		// список Избранное хранится только на клиенте
		return null;
	}

	@Override
	protected ArrayList<DataItem> loadAllDataFromServer() 
	{
		// список Избранное хранится только на клиенте
		return null;
	}

	@Override
	public DataControllerFactory selectDataItem(DataItem dataItem) 
	{
		// в зависимости от типа элемента возвращает соотв. дата-контроллер, 
		// чтобы был выполнен переход к "внутренностям" этого элемента
		switch (dataItem.getType())
		{
		case DataItem.TYPE_CATALOG:
			return DataControllerFactory.getCatalogsDCF().createDataController().selectDataItem(dataItem);
		
		case DataItem.TYPE_CATALOG_ITEM:
			return DataControllerFactory.getCatalogItemsDCF(
					dataItem.getParentId(), null, null).createDataController().selectDataItem(dataItem);
			
		case DataItem.TYPE_DOCUMENT:
			return DataControllerFactory.getDocumentsDCF().createDataController().selectDataItem(dataItem);
			
		default:
			return null;
		}
	}

	@Override
	public String getTitle()
	{
		return "Избранное";
	}
}
