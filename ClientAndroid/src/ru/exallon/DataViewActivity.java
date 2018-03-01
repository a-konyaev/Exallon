package ru.exallon;

import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

import ru.exallon.data.CatalogItemsDataController;
import ru.exallon.data.Data;
import ru.exallon.data.DataController;
import ru.exallon.data.DataControllerFactory;
import ru.exallon.data.DataItem;
import ru.exallon.data.FavoriteDataController;
import ru.exallon.data.SessionManager;
import ru.exallon.utils.LongOperation;
import ru.exallon.utils.MessageBox;
import ru.exallon.R;
import android.app.ListActivity;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.AdapterView;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.TableRow;
import android.widget.TextView;

/**
 * Базовая страница просмотра данных
 */
public class DataViewActivity extends ListActivity 
{
	/**
	 * Ключ экстра-данных "Фабрика дата-контроллера"
	 */
	public static final String EXTRA_DATA_CONTROLLER_FACTORY = "intent.data_view.extra.DATA_CONTROLLER_FACTORY";
	/**
	 * Контроллер данных
	 */
	private DataController _dataController;	

	/**
	 * Создает намерение для создания данной страницы
	 * @param context
	 * @param dcf фабрика дата-контроллера
	 * @return
	 */
	public static Intent getIntent(Context context, DataControllerFactory dcf)
	{
		Intent intent = new Intent(context, DataViewActivity.class);
		intent.putExtra(EXTRA_DATA_CONTROLLER_FACTORY, dcf);
		return intent;
	}
	
	/**
	 * Выполняет переход к странице, которая определяется фабрикой дата-контроллера.
	 * Если фабрика не задана, то ничего не происходит 
	 * @param dcf
	 */
	private void gotoActivity(DataControllerFactory dcf)
    {
    	if (dcf == null)
    		return;
    	
    	Intent intent = DataViewActivity.getIntent(this, dcf);
		startActivity(intent);
    }
	
//########################################################################################
//ИНИЦИАЛИЗАЦИЯ СТРАНИЦЫ
	
	@Override
	public void onCreate(Bundle savedInstanceState) 
	{
	    super.onCreate(savedInstanceState);
	    
	    setContentView(R.layout.data_view);	    
	    initDataController();
	    initTabs();
	    initFilter();
	    initDataList();
	}
	
	/**
     * Создание контекстного меню
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) 
    {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.data_view, menu);
        
        // пункт меню "Обновить" доступен только, если это не страница "Избранное"
        menu.findItem(R.id.menu_refresh).setEnabled(
        		_dataController.getType() != DataControllerFactory.CTRL_TYPE_FAVORITE);
        
        return true;
    }
    
    /**
     * Обработка выбора пункта меню
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) 
    {
    	switch (item.getItemId())
    	{
    	case R.id.menu_about:
    		startActivity(new Intent(this, AboutActivity.class));
    		return true;
    		
    	case R.id.menu_help:
    		startActivity(new Intent(this, HelpActivity.class));
    		return true;
    		
    	case R.id.menu_feedback:
    		startActivity(new Intent(this, FeedbackActivity.class));
    		return true;
        	
    	case R.id.menu_close_session:
    		closeSession();
        	return true;
        	
    	case R.id.menu_refresh:
    		refreshData();
    		return true;
        	
        default:
        	return false;
    	}
    }
    
    /**
	 * Закрытие текущей сессии
	 */
    private void closeSession()
    {
    	// закрываем сессию внутри долгой операции, 
    	// т.к. если сервер уже не доступен, то будем ждать таймаута
    	new LongOperation(
    			this,
    			"Завершение сеанса...",
    			new LongOperation.OperationHandler() 
    			{
					@Override
					public void processResult(Object result) 
					{
						// переходим на главную страницу
						Intent intent = new Intent(DataViewActivity.this, MainActivity.class);
						intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP);
						startActivity(intent);
					}
					
					@Override
					public Object execute() 
					{
						// закрываем сессию
						return SessionManager.getInstance().closeSession();
					}
				}).run();
    }
	
    /**
     * Обновить данные
     */
    public void refreshData() 
	{
    	MessageBox.show(
				this, 
				"Работа с данными",
				"Обновить данные?", 
				MessageBox.QUESTION,
				true,
				new DialogInterface.OnClickListener()
	            {
	                public void onClick(DialogInterface dialog, int whichButton) 
	                {
	                	refreshDataList(true);
	                }
	            },
	            null);
	}
    
	/**
	 * Инициализация контроллера данных
	 */
	private void initDataController()
	{
		// получим дата-контроллер
		DataControllerFactory dcf = this.getIntent().getParcelableExtra(EXTRA_DATA_CONTROLLER_FACTORY);
		_dataController = dcf.createDataController();
	}
	
	/**
	 * На текущей странице нужно отображать детальную информацию одного конкретного элемента?
	 */
	private boolean isDetailsView()
	{
		return DataControllerFactory.isDetailsController(_dataController.getType());
	}
	
	@Override
    protected void onResume() 
    {
		super.onResume();
    	
    	// выключаем режим фильтрации
		enableFilter(false);
		// обновим список
    	refreshDataList(false);
    }
	
	@Override
	protected void onPause() 
	{
		super.onPause();
		
		// запомним позицию списка
		saveListViewPosition();
		// спрячем клавиатуру
		showSoftKeyboard(false);
	}
	
	//TODO: может быть нужно в onStop говорить дата-контроллеру, чтобы освободил память?
	
//########################################################################################
//СПИСОК ЭЛЕМЕНТОВ
	
	/**
	 * Список элементов
	 */
	private ListView _listView;
	/**
	 * Индекс выделенного элемента списка
	 */
	private int _listViewIndex;
	/**
	 * Верхняя позиция списка
	 */
	private int _listViewTop;
	/**
	 * Заголовок
	 */
	private TextView _titleView;
	
	/**
	 * Инициализация списка данных
	 */
	private void initDataList()
	{
		_listView = this.getListView();
		
		// обработчик клика на элементе списка
    	_listView.setOnItemClickListener(new AdapterView.OnItemClickListener() 
        { 
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) 
            {
            	onDataItemClick((DataItem)parent.getItemAtPosition(position));
            }
		});
    	
    	// обработчик долгого клика на элементе списка
    	_listView.setOnItemLongClickListener(new AdapterView.OnItemLongClickListener()
        { 
            public boolean onItemLongClick(AdapterView<?> parent, View view, int position, long id) 
            {
            	onDataItemLongClick((DataItem)parent.getItemAtPosition(position));
            	return false;
            }
		});
    	
    	_titleView = (TextView)this.findViewById(R.id.data_view_title);
	}
	
	/**
	 * Сохранить текущую позицию списка
	 */
	private void saveListViewPosition()
	{
		_listViewIndex = _listView.getFirstVisiblePosition();
		View v = _listView.getChildAt(0);
		_listViewTop = (v == null) ? 0 : v.getTop();
	}
	
	/**
	 * Восстановить текущую позицию списка
	 */
	private void restoreListViewPosition()
	{
		_listView.setSelectionFromTop(_listViewIndex, _listViewTop);
	}
	
	/**
	 * Обновить список данных
	 * @param forceRefresh выполнить принудительное обновление данных с сервера
	 */
    private void refreshDataList(final boolean forceRefresh)
    {
    	new LongOperation(
    			this,
    			"Получение данных...",
    			new LongOperation.OperationHandler() 
    			{
					@Override
					public void processResult(Object result) 
					{
						fillDataList((Data)result);
					}
					
					@Override
					public Object execute() 
					{
						if (forceRefresh)
							_dataController.refresh();
						
						return _dataController.getData(_filterMode ? _filterText : null);
					}
				}).run();
    }
    
    /**
     * Заполнить список данных
     */
    private void fillDataList(Data data)
    {
    	// TODO фильтрация: см. listView.setFilterText
    	
    	if (data != null)
    	{
    		// заполним список
    		DataItemAdapter adapter = new DataItemAdapter(this, R.layout.data_item, data.getItems());
    		_listView.setAdapter(adapter);
    	
    		// установим заголовок
    		setTitle(data);
    	}
	    
    	// если при получении данных возникла ошибка
    	if (_dataController.hasError())
    	{
    		// то отобразим предупреждение
    		MessageBox.show(this, "Ошибка получения данных", 
    				_dataController.getError().getDescription(), MessageBox.ERROR);
    	}
    	
    	restoreListViewPosition();
    }
    
    /**
     * Установить заголовок
     */
    private void setTitle(Data data)
    {
    	StringBuilder sb = new StringBuilder(100);
		sb.append(_dataController.getTitle());
		
		if (data != null && !isDetailsView())
		{
			String size = Integer.toString(data.getItems().size());
			int suffixLen = size.length() + 3;
			int len = sb.length() + suffixLen;
			if (len > 48)
			{
				sb.setLength(48 - suffixLen - 3);
				sb.append("...");
			}
			
			sb.append(" (");
			sb.append(size);
			sb.append(')'); 
		}
		
    	_titleView.setText(sb.toString());
    }
    
    /**
     * Обработка клика на элементе списка
     * @param dataItem
     */
	private void onDataItemClick(DataItem dataItem)
	{
		gotoActivity(_dataController.selectDataItem(dataItem));
	}
	
	/**
	 * Обработка долгого клика на элементе списка
	 * @param dataItem
	 */
	private void onDataItemLongClick(final DataItem dataItem)
	{
		switch (_dataController.getType())
		{
		case DataControllerFactory.CTRL_TYPE_FAVORITE:
			MessageBox.show(
					this, 
					"Работа с данными",
					String.format("Удалить элемент '%s' из Избранного?", dataItem.getName()), 
					MessageBox.QUESTION,
					true,
					new DialogInterface.OnClickListener()
		            {
		                public void onClick(DialogInterface dialog, int whichButton)
		                {
		                	FavoriteDataController.removeDataItemFromFavorite(dataItem);
		                	refreshDataList(false);
		                }
		            },
		            null);
			return;
		
		case DataControllerFactory.CTRL_TYPE_CATALOG_ITEMS:
			addDataItemToFavorite(dataItem, ((CatalogItemsDataController)_dataController).getCatalogId());
			break;
			
		case DataControllerFactory.CTRL_TYPE_DOCUMENT_ITEMS:
			//TODO
			break;
			
		case DataControllerFactory.CTRL_TYPE_CATALOGS:			
		case DataControllerFactory.CTRL_TYPE_DOCUMENTS:
			addDataItemToFavorite(dataItem, null);
			break;
		}
	}
	
	/*
	 * Добавить элемент данных в Избранное
	 */
	private void addDataItemToFavorite(DataItem dataItem, String dataId)
	{
		FavoriteDataController.addDataItemToFavorite(dataItem, dataId);
		MessageBox.showToast(String.format("'%s' добавлен в Избранное", dataItem.getName()));
	}
	
//########################################################################################
//ФИЛЬТР
	
	private ImageView _filterShowButton;
	private LinearLayout _filterHideButton;
	private EditText _filterET;
	private TableRow _filterTableRow;
	private TableRow _tabTableRow;
	/**
	 * Признак того, что работаем в режиме фильтрации.
	 */
	private boolean _filterMode;
	/**
	 * Строка фильтра
	 */
	private String _filterText;
	
	/**
	 * Инициализация фильтра
	 */
	private void initFilter()
	{
		// получим ссылки на элементы управления
		_filterShowButton = (ImageView)this.findViewById(R.id.data_view_filter_show);
		_filterHideButton = (LinearLayout)this.findViewById(R.id.data_view_filter_hide);
		_filterET = (EditText)this.findViewById(R.id.data_view_filter);
		_filterTableRow = (TableRow)this.findViewById(R.id.data_view_filter_row);
		_tabTableRow = (TableRow)this.findViewById(R.id.data_view_tab_row);
		
		if (!isDetailsView())
		{		
			// добавление обработчика для события изменения строки фильтра
			_filterET.addTextChangedListener(new TextWatcher() {            
		        @Override
		        public void onTextChanged(CharSequence s, int start, int before, int count) 
		        {
		        	onFilterTextChanged(s);
		        }
		        @Override
		        public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
		        @Override
		        public void afterTextChanged(Editable s) {}
		      });
		}
	}
	
	/**
     * Обработка нажатия кнопки "Фильтр"
     */
    public void onShowFilterButtonClick(View view) 
	{
    	// включаем режима фильтрации
    	enableFilter(true);
	}
    
    /**
     * Обработка нажатия кнопки "Отмена" для закрытия фильтра
     */
    public void onHideFilterButtonClick(View view) 
	{
    	// выключаем режима фильтрации
    	enableFilter(false);
    	// обновим список данных
		refreshDataList(false);
	}
    
    /**
     * Включение/выключение режима фильтрации
     * @param enable
     */
    private void enableFilter(boolean enable)
    {
    	if (enable)
		{
    		// включаем режим фильтрации
			_filterTableRow.setVisibility(View.VISIBLE);
			_tabTableRow.setVisibility(View.GONE);
			_filterShowButton.setVisibility(View.GONE);
			_filterHideButton.setVisibility(View.VISIBLE);
			_filterET.requestFocus();
			showSoftKeyboard(true);
	    	_filterMode = true;
		}
		else
		{
			// вЫключаем режим фильтрации
			_filterMode = false;
			_filterTableRow.setVisibility(View.GONE);
			_tabTableRow.setVisibility(View.VISIBLE);
			_filterShowButton.setVisibility(isDetailsView() ? View.GONE : View.VISIBLE);
			_filterHideButton.setVisibility(View.GONE);
			_filterET.setText("");
	    	_filterText = "";
			showSoftKeyboard(false);
		}
    }
    
    /**
     * Отобразить/скрыть клавиатуру
     * @param show
     */
    private void showSoftKeyboard(boolean show)
    {
    	InputMethodManager mgr = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
    	if (show)
    		mgr.showSoftInput(_filterET, InputMethodManager.SHOW_FORCED);
    	else
    		mgr.hideSoftInputFromWindow(_filterET.getWindowToken(), 0);
    }
	
	/**
	 * Обработка изменения строки фильтра
	 */
	private void onFilterTextChanged(CharSequence s)
	{
		if (!_filterMode)
			return;
		
		_filterText = s.toString();
		refreshDataList(false);
	}
	
	/**
     * Обработка нажатия кнопки "Очистить строку фильтра"
     */
    public void onClearFilterButtonClick(View view) 
	{
    	_filterET.setText("");
    	_filterText = "";
    	refreshDataList(false);
	}

//########################################################################################
//ЗАКЛАДКИ
    
	/**
	 * Таблица соотвествия дата-контроллеров и параметров закладок
	 * [{типы дата-контроллеров}, {ИД закладки, ИД on-картинки, ИД off-картинки}]
	 */
	private static final Map<int[], int[]> _dataCtrlsToTabParamsMap;
	static {
		Map<int[], int[]> map = new HashMap<int[], int[]>();
		map.put(
				new int[] {
						DataControllerFactory.CTRL_TYPE_FAVORITE},
				new int[] {
						R.id.data_view_tab_favorite, 
						R.drawable.tab_favorite_on,
						R.drawable.tab_favorite_off });
		
		map.put(
				new int[] {
						DataControllerFactory.CTRL_TYPE_CATALOGS,
						DataControllerFactory.CTRL_TYPE_CATALOG_ITEMS,
						DataControllerFactory.CTRL_TYPE_CATALOG_ITEM_DETAILS}, 
				new int[] {
						R.id.data_view_tab_catalog, 
						R.drawable.tab_catalog_on,
						R.drawable.tab_catalog_off });
		
		map.put(
				new int[] {
						DataControllerFactory.CTRL_TYPE_DOCUMENTS}, 
				new int[] {
						R.id.data_view_tab_document, 
						R.drawable.tab_document_on,
						R.drawable.tab_document_off });
		
		_dataCtrlsToTabParamsMap = Collections.unmodifiableMap(map);
	}
	
	/**
	 * Инициализация закладок
	 */
	private void initTabs()
	{
		int ctrlType = _dataController.getType();
		
		for (Map.Entry<int[], int[]> entry : _dataCtrlsToTabParamsMap.entrySet()) 
		{
        	boolean enabled = false;
        	for (int t : entry.getKey())
        	{
        		if (t == ctrlType)
        		{
        			enabled = true;
        			break;
        		}
        	}
        	
        	int[] res = entry.getValue();
        	ImageView button = (ImageView)this.findViewById(res[0]);
        	button.setImageResource(enabled ? res[1] : res[2]);
        }
	}
	
    /**
     * Обработка выбора закладки "Избранное"
     */
    public void onFavoriteTabClick(View view) 
	{
    	gotoActivity(DataControllerFactory.getFavoriteDCF());
	}
    
    /**
     * Обработка выбора закладки "Справочники"
     */
    public void onCatalogTabClick(View view) 
	{
    	gotoActivity(DataControllerFactory.getCatalogsDCF());
	}
    
    /**
     * Обработка выбора закладки "Документы"
     */
    public void onDocumentTabClick(View view) 
	{
    	gotoActivity(DataControllerFactory.getDocumentsDCF());
	}
}
