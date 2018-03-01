package ru.exallon.data;

import java.util.ArrayList;

import ru.exallon.Application;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

/**
 * Менеджер кэша
 */
public class CacheManager 
{
	private static String SUB_LOG_TAG = "CacheManager";
	private static CacheManager _instance;
	/**
	 * Экземпляр класса
	 */
	public static synchronized CacheManager getInstance()
	{
		if (_instance == null)
			_instance = new CacheManager();
		
		return _instance;
	}
	
	/**
	 * Менеджер сессий
	 */
	private SessionManager _sessionManager;
	/**
	 * Посредник для работы с БД
	 */
	private DbHelper _dbHelper;
	/**
	 * БД
	 */
	private SQLiteDatabase _db;
	
	/**
	 * Конструктор
	 */
	private CacheManager()
	{
		_sessionManager = SessionManager.getInstance();
		
		_dbHelper = new DbHelper(Application.getContext());
		_db = _dbHelper.getWritableDatabase();
	}

	/**
	 * Деструктор
	 */
	public static synchronized void Close()
	{
		if (_instance == null)
			return;
		
		_instance._dbHelper.close();
		_instance = null;
	}

	/**
	 * Посредник для взаимодействия с БД
	 */
	private class DbHelper extends SQLiteOpenHelper 
	{
		private static final String _dbName = "cache";
		private static final int _dbVersion = 4;

		public DbHelper(Context context) {
			super(context, _dbName, null, _dbVersion);
		}

		/**
		 * Создание БД
		 */
		@Override
		public void onCreate(SQLiteDatabase db) 
		{
			db.execSQL("CREATE TABLE Data (" +
					"ServerId TEXT(36) NOT NULL, " +	// ИД сервера
					"CacheId TEXT(36) NOT NULL, " + 	// ИД кэша
					"Id TEXT(36) NOT NULL, " +			// ИД записи
					"Type INTEGER NOT NULL, " +			// Тип записи 
					"Name TEXT, " +						// Наименование
					"Value TEXT, " +					// Значение
					//TODO: используется только для того, чтобы выполнять регистронезависимый поиск, 
					//т.к. в SQLite для unicode поиск всегда регистрозависимый
					"Search TEXT, " +					// Name & Value в нижнем регистре
					"ParentId TEXT(36), " + 			// ИД родительской записи
					"IsGroup INTEGER NOT NULL)");		// признак того, что данная запись имеет дочерние записи
		}

		/**
		 * Обновление БД
		 */
		@Override
		public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) 
		{
			db.execSQL("DROP TABLE IF EXISTS Data");
			onCreate(db);
		}
	}	

	/**
	 * Получить данные из кэша
	 * @param cacheId идентификатор кэша
	 * @return
	 */
	public Data getData(String cacheId, String searchMask, String parentId) 
	{		
		if (isCacheEmpty(cacheId))
		{
			Application.logVerbose(SUB_LOG_TAG, "getData: cacheId=%s: cache is empty", cacheId);
			return null;
		}
		
		Cursor cur = null;
        try
        {
        	StringBuilder sb = new StringBuilder(256);
        	if (searchMask != null)
        	{
        		sb.append("Search LIKE '%");
        		sb.append(searchMask.toLowerCase());
        		sb.append("%' AND ");
        	}
        	
        	if (parentId != null)
        	{
        		sb.append("ParentId =  '");
        		sb.append(parentId);
        		sb.append("' AND ");
        	}
        	
        	sb.append("ServerId = '");
        	sb.append(_sessionManager.getActiveServerId());
        	sb.append("' AND CacheId = '");
        	sb.append(cacheId);
        	sb.append("'");
        	
        	String where = sb.toString();

        	cur = _db.query(
    				false, 
    				"Data", 
    				new String[] { "Id", "Type", "Name", "Value", "ParentId", "IsGroup"}, 
    				where, null, null, null, "IsGroup DESC, Name", null);
        	
    		Data data = new Data();
    		data.setAllDataLoaded(true);
    		
    		ArrayList<DataItem> items = data.getItems();
    		cur.moveToFirst();
            while (cur.isAfterLast() == false) {
            	DataItem item = new DataItem(
            			cur.getString(0),
            			cur.getInt(1),
            			cur.getString(2),
            			cur.getString(3),
            			cur.getString(4),
            			cur.getInt(5) == 1);
                items.add(item);
           	    cur.moveToNext();
            }
            
            return data;
        }
        catch (Exception ex)
        {
        	Application.logError(
        			SUB_LOG_TAG, ex, 
        			"getData failed: cacheId=%s; searchMask=%s; parentId=%s", 
        			cacheId, searchMask, parentId); 
        	return null;
        }
        finally
        {
        	if (cur != null)
        		cur.close();
        }
	}
	
	/**
	 * Пуст ли кэш
	 * @param cacheId идентификатор кэша
	 * @return
	 */
	private boolean isCacheEmpty(String cacheId)
	{		
		Cursor cur = null;
		try
		{
			cur = _db.rawQuery(
					"SELECT COUNT(*) FROM Data WHERE ServerId=? AND CacheId=?",
					new String[] { _sessionManager.getActiveServerId(), cacheId });
			cur.moveToFirst();
			return cur.getInt(0) == 0;
		}
		catch (Exception ex)
        {
			Application.logError(SUB_LOG_TAG, ex, "isCacheEmpty failed: cacheId=%s", cacheId);
        	return true;
        }
		finally
        {
        	if (cur != null)
        		cur.close();
        }
	}

	/**
	 * Добавить порцию данных в кэш
	 * @param cacheId идентификатор кэша
	 * @param chank порция данных
	 * @param lastChank признак того, что это последняя порция
	 */
	public void addDataChank(
			String cacheId, 
			ArrayList<DataItem> chank,
			boolean lastChank) 
	{
		try 
		{
			String serverId = _sessionManager.getActiveServerId();
			
			for (DataItem item : chank) 
			{
				ContentValues values = new ContentValues(7);
				values.put("ServerId", serverId);
		        values.put("CacheId", cacheId);
		        values.put("Id", item.getId());
		        values.put("Type", item.getType());
		        values.put("Name", item.getName());
		        values.put("Value", item.getValue()); 
		        values.put("Search", String.format("%s %s", item.getName(), item.getValue()).toLowerCase());
		        
		        values.put("ParentId", item.getParentId());
		        values.put("IsGroup", item.getIsGroup() ? "1" : "0");
		        
				_db.insert("Data", null, values);
			}
		} 
		catch (Exception ex)
		{
			Application.logError(SUB_LOG_TAG, ex, "addDataChank failed: cacheId=%s", cacheId);
		}
	}
	
	/**
	 * Удалить заданный элемент из кэша
	 * @param cacheId
	 * @param item
	 */
	public void removeDataItem(String cacheId, DataItem item)
	{
		try 
		{
			_db.delete(
					"Data", 
					"ServerId=? AND CacheId=? AND Id=?", 
					new String[] { _sessionManager.getActiveServerId(), cacheId, item.getId() });
		}
		catch (Exception ex)
		{
			Application.logError(SUB_LOG_TAG, ex, "removeDataItem failed: cacheId=%s", cacheId);
		}
	}
	
	/**
	 * Выполняет поиск элемента с заданным идентификатором
	 * @param cacheId
	 * @param itemId
	 * @return элемент или null, если найти не удалось
	 */
	public DataItem findDataItem(String cacheId, String itemId)
	{
		Cursor cur = null;
		try
		{
			cur = _db.rawQuery(
					"SELECT Type, Name, Value, ParentId, IsGroup FROM Data WHERE ServerId=? AND CacheId=? AND Id=?",
					new String[] { _sessionManager.getActiveServerId(), cacheId, itemId });
			cur.moveToFirst();
			
			// если элемент с таким идентификатором не найден
			if (cur.isAfterLast())
				return null;

			return new DataItem(
					itemId,
					cur.getInt(0),
        			cur.getString(1),
        			cur.getString(2),
        			cur.getString(3),
        			cur.getInt(4) == 1);
		}
		catch (Exception ex)
        {
			Application.logError(SUB_LOG_TAG, ex, "findDataItem failed: cacheId=%s; itemId=%s", cacheId, itemId);
        	return null;
        }
		finally
        {
        	if (cur != null)
        		cur.close();
        }
	}
	
	/**
	 * Очистить кэш
	 * @param cacheId идентификатор кэша
	 */
	public void clear(String cacheId) 
	{
		try 
		{
			_db.delete(
					"Data", 
					"ServerId=? AND CacheId=?", 
					new String[] { _sessionManager.getActiveServerId(), cacheId });
		} 
		catch (Exception ex)
		{
			Application.logError(SUB_LOG_TAG, ex, "clear failed: cacheId=%s", cacheId);
		}
	}
	
	/**
	 * Удалить кэш
	 * @param serverId идентификатор сервера, данные с которого нужно удалить
	 */
	public void remove(String serverId) 
	{
		try 
		{
			_db.delete(
					"Data", 
					"ServerId=?", 
					new String[] { serverId });
		} 
		catch (Exception ex)
		{
			Application.logError(SUB_LOG_TAG, ex, "remove failed: serverId=%s", serverId); 
		}
	}
}
