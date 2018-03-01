package ru.exallon;

import ru.exallon.data.CacheManager;
import ru.exallon.data.ConfigManager;
import ru.exallon.data.DataControllerFactory;
import ru.exallon.data.DatabaseSettings;
import ru.exallon.data.SessionManager;
import ru.exallon.server.SessionResponse;
import ru.exallon.server.ServerError;
import ru.exallon.utils.LongOperation;
import ru.exallon.utils.MessageBox;
import ru.exallon.R;

import android.app.AlertDialog;
import android.app.ListActivity;
import android.os.Bundle;
import android.content.DialogInterface;
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.ListView;
import android.widget.EditText;

public class MainActivity extends ListActivity
{
	private ConfigManager _configManager;
	private DatabaseSettings _selectedDbs;
	private SessionManager _sessionManager;
	
    @Override
    public void onCreate(Bundle savedInstanceState) 
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        
        _configManager = ConfigManager.getInstance();
		_sessionManager = SessionManager.getInstance();
    }
    
    /**
     * Создание контекстного меню
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) 
    {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.main, menu);
        return true;
    }
    
    /**
     * Обработка выбора пункта меню
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) 
    {
    	int menuId = item.getItemId();
    	Class<?> activityClass = null;
    	
    	switch (menuId)
    	{
    	case R.id.menu_about:
    		activityClass = AboutActivity.class;
    		break;
    		
    	case R.id.menu_help:
    		activityClass = HelpActivity.class;
    		break;
    		
    	case R.id.menu_feedback:
    		activityClass = FeedbackActivity.class;
        	break;
        	
        default:
        	return false;
    	}
    	
    	startActivity(new Intent(this, activityClass));    	
    	return super.onOptionsItemSelected(item);
    }
    
    @Override
    protected void onStart() 
    {
    	super.onStart();
    	
    	new LongOperation(
    			this,
    			"Завершение сеанса...",
    			new LongOperation.OperationHandler() 
    			{
					@Override
					public void processResult(Object result) 
					{
						// заполняем список БД
				    	fillDBList();
					}
					
					@Override
					public Object execute() 
					{
						// закрываем сессию на случай, если 
						// мы вышли на главное окно путем нажатия кнопки Назад
						return SessionManager.getInstance().closeSession();
					}
				}).run();
    }
    
    @Override
    protected void onDestroy() 
    {
    	super.onDestroy();
    	// закрываем взаимодействие с БД
    	CacheManager.Close();
    }
    
    /**
     * Заполнение списка настроенных БД
     */
    private void fillDBList()
    {
    	// заполним список БД
		ListView listView = this.getListView();
    	listView.setAdapter(new ArrayAdapter<DatabaseSettings>(
    			this, 
    			R.layout.db_item,
    			R.id.db_item_text,
				_configManager.getDatabaseSettings()));
    	
    	// обработчик клика на элементе списка
    	listView.setOnItemClickListener(new AdapterView.OnItemClickListener() 
        { 
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) 
            {
            	onConnectToDatabase((DatabaseSettings)parent.getItemAtPosition(position), false);
            }
		});
    	
    	// обработчик долгого клика на элементе списка
    	listView.setOnItemLongClickListener(new AdapterView.OnItemLongClickListener()
        { 
            public boolean onItemLongClick(AdapterView<?> parent, View view, int position, long id) 
            {
            	onEditDatabaseSettings((DatabaseSettings)parent.getItemAtPosition(position));
            	return false;
            }
		});
    }
    
    /**
     * Подключение к БД
     */
    private void onConnectToDatabase(DatabaseSettings dbs, boolean enquiryIdentity)
    {
    	_selectedDbs = dbs;
    	
    	// если для данной БД не автовход или все равно нужно запросить идентификационные данные
    	if (!dbs.AutoConnect || enquiryIdentity)
    	{
    		// то запросим логин и пароль
    		final View dialogView = LayoutInflater.from(this).inflate(R.layout.login, null);
            final EditText usernameET = (EditText)dialogView.findViewById(R.id.login_username);
            final EditText passwordET = (EditText)dialogView.findViewById(R.id.login_password);
            final CheckBox autoconnectCB = (CheckBox)dialogView.findViewById(R.id.login_autoconnect);
            
            usernameET.setText(dbs.Username);
            autoconnectCB.setChecked(dbs.AutoConnect);
            
            new AlertDialog.Builder(this)
                .setIcon(R.drawable.msg_box_key)
                .setTitle("Авторизация")
                .setView(dialogView)
                .setPositiveButton("OK", new DialogInterface.OnClickListener()
                {
                    public void onClick(DialogInterface dialog, int whichButton)
                    {
                    	_selectedDbs.Username = usernameET.getText().toString();;
                    	_selectedDbs.Password = passwordET.getText().toString();
                    	_selectedDbs.AutoConnect = autoconnectCB.isChecked();
                    	_configManager.save();
                    	
                    	connectToDatabase();
                    }
                })
                .show();
            return;
    	}
    	
    	connectToDatabase();
    }
    
    /**
     * Устанавливает соединение с сервером
     */
    private void connectToDatabase()
    {
    	new LongOperation(
    			this,
    			"Подключение к серверу...",
    			new LongOperation.OperationHandler() 
    			{
					@Override
					public void processResult(Object result) 
					{
						connectToDatabaseDone((SessionResponse)result);
					}
					
					@Override
					public Object execute() 
					{
						return _sessionManager.openSession(_selectedDbs);
					}
				}).run();
    }
    
    /**
     * Обработка результата подключения к серверу
     */
    private void connectToDatabaseDone(SessionResponse resp)
    {
    	ServerError error = resp.getError();
    	
    	if (error == null)
    	{
    		Intent intent = DataViewActivity.getIntent(this, DataControllerFactory.getDocumentsDCF());
			startActivity(intent);
    		return;
    	}
    	
		if (error.getCode() == ServerError.SERVER_UNAUTHORIZED)
		{
			MessageBox.show(
					this, 
					"Ошибка авторизации",
					error.getDescription() + "\nПопробовать ещё раз?", 
					MessageBox.ERROR,
					true,
					new DialogInterface.OnClickListener() 
					{
						public void onClick(DialogInterface dialog, int whichButton) 
						{
							onConnectToDatabase(_selectedDbs, true);
						}
					},
					null);
		}
		else
		{
			MessageBox.show(this, "Ошибка авторизации", error.getDescription(), MessageBox.ERROR);
		}
    }
    
    /**
     * Редактирование настройки БД
     */
    private void onEditDatabaseSettings(DatabaseSettings dbs)
    {
    	int dsIndex = _configManager.getDatabaseSettings().indexOf(dbs);
    	Intent intent = DBSettingsActivity.getIntent(this, dsIndex);    	
    	startActivity(intent);
    }
    
    /**
     * Обработка нажатия кнопки "Добавить БД"
     */
    public void onAddButtonClick(View view)
    {
    	startActivity(new Intent(this, DBSettingsActivity.class));
    }
}