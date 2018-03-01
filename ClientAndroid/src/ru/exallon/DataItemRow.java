package ru.exallon;

import ru.exallon.data.DataItem;
import ru.exallon.R;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;

public class DataItemRow 
{
	private ImageView _image;
	private TextView _header;
	private TextView _body;
	
	public DataItemRow(ImageView image, TextView header, TextView body)
	{
		_image = image;
		_header = header;
		_body = body;
	}

	/**
	 * Связать строку с элементом данных
	 * @param item
	 */
	public void bind(DataItem item) 
	{
		switch (item.getType())
		{
		case DataItem.TYPE_CATALOG:
			_image.setImageResource(R.drawable.data_item_catalogs);
			_image.setVisibility(View.VISIBLE);
			_header.setVisibility(View.GONE);
			_body.setText(item.getName());
			break;
			
		case DataItem.TYPE_CATALOG_ITEM:
			int imgRes = (item.getIsGroup() 
					? R.drawable.data_item_catalog_group 
					: R.drawable.data_item_catalog_item); 
			_image.setImageResource(imgRes);
			_image.setVisibility(View.VISIBLE);
			_header.setVisibility(View.GONE);
			_body.setText(item.getName());
			break;
			
		case DataItem.TYPE_CATALOG_ITEM_PROPERTY:
			_image.setVisibility(View.GONE);
			_header.setVisibility(View.VISIBLE);
			_header.setText(item.getName());
			_body.setText(item.getValue());
			break;
			
		case DataItem.TYPE_DOCUMENT:
			_image.setImageResource(R.drawable.data_item_documents);
			_image.setVisibility(View.VISIBLE);
			_header.setVisibility(View.GONE);
			_body.setText(item.getName());
			break;

		default:
			_image.setVisibility(View.GONE);
			_header.setVisibility(View.GONE);
			_body.setText(item.getName());
			break;
		}
	}
}
