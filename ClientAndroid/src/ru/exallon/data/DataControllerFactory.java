package ru.exallon.data;

import android.os.Parcel;
import android.os.Parcelable;

/**
 * Фабрика дата-контроллеров
 * 
 * В зависимости от информации, которую хранит фабрика, 
 * она создает контроллеры соотв. типов.
 */
public class DataControllerFactory implements Parcelable 
{
	/**
	 * Тип контроллера "Избранное"
	 */
    public static final int CTRL_TYPE_FAVORITE = 10;
    
	/**
	 * Тип контроллера "Справочники"
	 */
    public static final int CTRL_TYPE_CATALOGS = 20;
    /**
	 * Тип контроллера "Элементы справочника"
	 */
    public static final int CTRL_TYPE_CATALOG_ITEMS = 21;
    /**
	 * Тип контроллера "Детальная информация элемента справочника"
	 */
    public static final int CTRL_TYPE_CATALOG_ITEM_DETAILS = 22;
    
    /**
	 * Тип контроллера "Документы"
	 */
    public static final int CTRL_TYPE_DOCUMENTS = 30;
    /**
	 * Тип контроллера "Элементы документа"
	 */
    public static final int CTRL_TYPE_DOCUMENT_ITEMS = 31;
    /**
	 * Тип контроллера "Детальная информация элемента документа"
	 */
    public static final int CTRL_TYPE_DOCUMENT_ITEM_DETAILS = 32;
    
    
    /**
	 * Заданный тип контроллера предоставляет детальную информацию по элементу?
	 */
    public static boolean isDetailsController(int ctrlType)
    {
    	return ctrlType == CTRL_TYPE_CATALOG_ITEM_DETAILS || ctrlType == CTRL_TYPE_DOCUMENT_ITEM_DETAILS;
    }
    
    private int _ctrlType;
    /**
     * Тип контроллера
     */
    public int getControllerType()
    {
    	return _ctrlType;
    }
    
    /**
     * Название типа контроллера
     */
    public static String getControllerTypeName(int ctrlType)
    {
    	switch (ctrlType)
		{
		case CTRL_TYPE_FAVORITE:
			return "Favorite";
			
		case CTRL_TYPE_CATALOGS:
			return "Catalogs";
			
		case CTRL_TYPE_CATALOG_ITEMS:
			return "CatalogItems";
			
		case CTRL_TYPE_CATALOG_ITEM_DETAILS:
			return "CatalogItemDetails";
			
		case CTRL_TYPE_DOCUMENTS:
			return "Documents";
			
		default:
			return null;
		}
    }
    
    /**
     * Параметры, которые использует фабрика при создании контроллера 
     */
    private String[] _params;   
    
    /**
     * Закрытый конструктор
     * @param ctrlType тип дата-контроллера
     * @param params параметры для инициализации дата-контроллера 
     */
    private DataControllerFactory(int ctrlType, String[] params)
    {
    	_ctrlType = ctrlType;
    	_params = params;
    }
    
    /**
     * Создает фабрику контроллеров типа "Избранное"
     * @return
     */
    public static DataControllerFactory getFavoriteDCF()
    {
    	return new DataControllerFactory(CTRL_TYPE_FAVORITE, null);
    }
    
    /**
     * Создает фабрику контроллеров типа "Справочники"
     * @return
     */
    public static DataControllerFactory getCatalogsDCF()
    {
    	return new DataControllerFactory(CTRL_TYPE_CATALOGS, null);
    }
    
    /**
     * Создает фабрику контроллеров типа "Элементы Справочника"
     * @return
     */
    public static DataControllerFactory getCatalogItemsDCF(
    		String catalogId, String name, String parentItemId)
    {
    	return new DataControllerFactory(
    			CTRL_TYPE_CATALOG_ITEMS, 
    			new String[]{catalogId, name, parentItemId});
    }
    
    /**
     * Создает фабрику контроллеров типа "Детальная информация элемента справочника"
     * @return
     */
    public static DataControllerFactory getCatalogItemDetailsDCF(
    		String catalogId, String itemId, String name)
    {
    	return new DataControllerFactory(
    			CTRL_TYPE_CATALOG_ITEM_DETAILS, 
    			new String[]{catalogId, itemId, name});
    }
    
    /**
     * Создает фабрику контроллеров типа "Документы"
     * @return
     */
    public static DataControllerFactory getDocumentsDCF()
    {
    	return new DataControllerFactory(CTRL_TYPE_DOCUMENTS, null);
    }
    
	/**
	 * Создает дата-контроллер
	 * @return
	 */
	public DataController createDataController()
	{
		switch (_ctrlType)
		{
		case CTRL_TYPE_FAVORITE:
			return new FavoriteDataController(CTRL_TYPE_FAVORITE);
			
		case CTRL_TYPE_CATALOGS:
			return new CatalogsDataController(CTRL_TYPE_CATALOGS);
			
		case CTRL_TYPE_CATALOG_ITEMS:
			return new CatalogItemsDataController(
					CTRL_TYPE_CATALOG_ITEMS, _params[0], _params[1], _params[2]);
			
		case CTRL_TYPE_CATALOG_ITEM_DETAILS:
			return new CatalogItemDetailsDataController(
					CTRL_TYPE_CATALOG_ITEM_DETAILS, _params[0], _params[1], _params[2]);
			
		case CTRL_TYPE_DOCUMENTS:
			return new DocumentsDataController(CTRL_TYPE_DOCUMENTS);
			
		default:
			return null;
		}
	}

//########################################################################################
//Реализация Parcelable
	
	public static final Parcelable.Creator<DataControllerFactory> CREATOR 
	= new Parcelable.Creator<DataControllerFactory>() 
	{
		public DataControllerFactory createFromParcel(Parcel in) 
		{
			return new DataControllerFactory(in);         
		}
		
		public DataControllerFactory[] newArray(int size) 
		{             
			return new DataControllerFactory[size];         
		}     
	};          
	
	private DataControllerFactory(Parcel in) 
	{
		_ctrlType = in.readInt();
		
		switch (_ctrlType)
		{			
		case CTRL_TYPE_CATALOG_ITEMS:
			_params = new String[3];
			in.readStringArray(_params);
			break;
			
		case CTRL_TYPE_CATALOG_ITEM_DETAILS:
			_params = new String[3];
			in.readStringArray(_params);
			break;
		}
	}
	
	@Override
	public int describeContents() 
	{
		return 0;
	}

	@Override
	public void writeToParcel(Parcel out, int flags) 
	{
		out.writeInt(_ctrlType);
		if (_params != null)
			out.writeStringArray(_params);
	}
}
