package ru.exallon.server;

import java.util.ArrayList;

import org.ksoap2.serialization.SoapObject;

import ru.exallon.data.DataItem;

/**
 * Ответ, содержащий данные
 */
public class DataResponse extends Response
{
	private int _dataType;
	/**
	 * Тип данных
	 */
	public int getDataType()
	{
		return _dataType;
	}
	
	private ArrayList<DataItem> _data;
	/**
	 * Данные
	 */
	public ArrayList<DataItem> getData()
	{
		return _data;
	}
	
	public DataResponse(int dataType) 
	{
		_dataType = dataType;
	}
	
	public static DataResponse getFailedResponse(int errorCode) 
	{
		DataResponse resp = new DataResponse(-1);
		resp.setError(errorCode);
		return resp;
	}
	
	@Override
	public void deserializeProperty(int index, Object value) throws Exception
	{
		if (index != 0)
			throw new Exception("Неожиданный индекс св-ва");
		
        SoapObject so = (SoapObject)value;
        int count = so.getPropertyCount();
        
        _data = new ArrayList<DataItem>(count);
        
        for (int i = 0; i < count; i++)
        {
        	SoapObject so2 = (SoapObject)so.getProperty(i);
        	
        	Object name = so2.getProperty("Name");
        	Object parentId = so2.getProperty("ParentId");
        	Object val = so2.getProperty("Value");
        	
        	DataItem item = new DataItem(
        			so2.getProperty("Id").toString(),
        			_dataType,
        			name == null ? null : name.toString(),
        			val == null ? null : val.toString(),
        			parentId == null ? null : parentId.toString(),
        			Boolean.parseBoolean(so2.getProperty("IsGroup").toString()));
            _data.add(item);
        }
	}
}
