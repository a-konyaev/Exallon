package ru.exallon.data;

/**
 * Элемент данных
 */
public class DataItem 
{
	/**
	 * Пустой идентификатор
	 */
	public static final String EMPTY_ID = "00000000-0000-0000-0000-000000000000";
	
	/**
	 * Тип элемента данных "Справочник"
	 */
    public static final int TYPE_CATALOG = 1;
    /**
	 * Тип элемента данных "Элемент справочника"
	 */
    public static final int TYPE_CATALOG_ITEM = 2;
    /**
	 * Тип элемента данных "Реквизит элемента справочника"
	 */
    public static final int TYPE_CATALOG_ITEM_PROPERTY = 3;
    /**
	 * Тип элемента данных "Документ"
	 */
    public static final int TYPE_DOCUMENT = 4;
    
	private String _id;
	/**
	 * Идентификатор
	 */
	public String getId()
	{
		return _id;
	}
	
	private int _type;
	/**
	 * Тип элемента данных
	 */
	public int getType()
	{
		return _type;
	}
	
	private String _name;
	/**
	 * Наименование
	 */
	public String getName()
	{
		return _name;
	}
	
	private String _value;
	/**
	 * Значение
	 */
	public String getValue()
	{
		return _value;
	}
	
	private String _parentId;
	/**
	 * Идентификатор родительского элемента
	 */
	public String getParentId()
	{
		return _parentId;
	}
	
	private boolean _isGroup;
	/**
	 * Является ли данный элемент группой, т.е. содержит ли дочерние элементы
	 */
	public boolean getIsGroup()
	{
		return _isGroup;
	}
	
	/**
	 * Конструктор
	 */
	public DataItem(String id, int type, String name, String value, String parentId, boolean isGroup)
	{
		_id = id;
		_type = type;
		_name = name;
		_value = value;
		_parentId = parentId;
		_isGroup = isGroup;
	}
}
