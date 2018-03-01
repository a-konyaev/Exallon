package ru.exallon;

import java.util.ArrayList;

import ru.exallon.data.DataItem;
import ru.exallon.R;
import android.app.Activity;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

public class DataItemAdapter extends ArrayAdapter<DataItem> 
{
	private Context _context;
	private int _layoutResourceId;
	private ArrayList<DataItem> _items;
	
	public DataItemAdapter(Context context, int layoutResourceId, ArrayList<DataItem> items) 
	{
		super(context, layoutResourceId, items);
		_context = context;
		_layoutResourceId = layoutResourceId;
		_items = items;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent) 
	{
		View rowView = convertView;
        
        if(rowView == null)
        {
            LayoutInflater inflater = ((Activity)_context).getLayoutInflater();
            rowView = inflater.inflate(_layoutResourceId, parent, false);
        }
        
        if(rowView == null)
        {
            LayoutInflater inflater = ((Activity)_context).getLayoutInflater();
            rowView = inflater.inflate(_layoutResourceId, parent, false);
        }
        
        DataItemRow row = (DataItemRow)rowView.getTag();
        if (row == null)
        {
            row = new DataItemRow(
            		(ImageView)rowView.findViewById(R.id.data_item_image),
            		(TextView)rowView.findViewById(R.id.data_item_header),
            		(TextView)rowView.findViewById(R.id.data_item_body));
            rowView.setTag(row);
        }
        
        DataItem item = _items.get(position);
        row.bind(item);
        
        return rowView;
	}
}
