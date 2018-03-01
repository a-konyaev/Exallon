package ru.exallon;

import ru.exallon.data.CacheManager;
import ru.exallon.data.ConfigManager;
import ru.exallon.data.DatabaseSettings;
import ru.exallon.utils.LongOperation;
import ru.exallon.utils.MessageBox;
import ru.exallon.R;

import android.app.Activity;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;

import android.view.View;
import android.webkit.URLUtil;
import android.widget.CheckBox;
import android.widget.EditText;

public class DBSettingsActivity extends Activity 
{
	private static final String EXTRA_INDEX = "intent.db_settings.extra.INDEX";
	
	private CacheManager _cacheManager;
	
	private ConfigManager _configManager;
	private DatabaseSettings _dbSettings;
	private int _index;
	
	private EditText _nameET;
	private EditText _serverUrlET;
	private CheckBox _autoconnectCB;
	private EditText _usernameET;
	private EditText _passwordET;
	
	
	/**
	 * Создает намерение для создания данной страницы
	 * @param context контекст
	 * @param dsIndex индекс настройки БД
	 * @return
	 */
	public static Intent getIntent(Context context, int dsIndex)
	{
		Intent intent = new Intent(context, DBSettingsActivity.class);
    	intent.putExtra(DBSettingsActivity.EXTRA_INDEX, dsIndex);
    	return intent;
	}
	
	/**
	 * Создание страницы
	 */
	@Override
	public void onCreate(Bundle savedInstanceState) 
	{
	    super.onCreate(savedInstanceState);
	    setContentView(R.layout.db_settings);
	    
	    _cacheManager = CacheManager.getInstance();
	    _configManager = ConfigManager.getInstance();	    
	    
	    _index = this.getIntent().getIntExtra(EXTRA_INDEX, -1);
		_dbSettings = (_index == -1
				? DatabaseSettings.New() 
				: _configManager.getDatabaseSettings().get(_index));
		
		// выключаем кнопки "Очистить БД" и "Удалить", если это создание новой настройки БД
		boolean enabled = (_index != -1);
		((View)this.findViewById(R.id.db_settings_delete)).setEnabled(enabled);
		((View)this.findViewById(R.id.db_settings_clear)).setEnabled(enabled);
		
		// получаем ссылки на элементы управления
		_nameET = (EditText)this.findViewById(R.id.db_settings_name);
		_serverUrlET = (EditText)this.findViewById(R.id.db_settings_server_url);
		_autoconnectCB = (CheckBox)this.findViewById(R.id.db_settings_autoconnect);
		_usernameET = (EditText)this.findViewById(R.id.db_settings_username);
		_passwordET = (EditText)this.findViewById(R.id.db_settings_password);
		
		// заполняем значениями элементы управления
		fillView();
	}
	
	private void fillView()
	{
		_nameET.setText(_dbSettings.Name);
		_serverUrlET.setText(_dbSettings.ServerUrl);
		_autoconnectCB.setChecked(_dbSettings.AutoConnect);
		_usernameET.setText(_dbSettings.Username);
		_passwordET.setText(_dbSettings.Password);
		
		onAutoconnectCheckboxClick(null);
	}
	
	private void readDataFromView()
	{
		_dbSettings.Name = _nameET.getText().toString();
		_dbSettings.ServerUrl = _serverUrlET.getText().toString();
		_dbSettings.Username = _usernameET.getText().toString();
		_dbSettings.AutoConnect = _autoconnectCB.isChecked();
		
		if (_dbSettings.AutoConnect)
		{
			_dbSettings.Password = _passwordET.getText().toString();
		}
		else
		{
			_dbSettings.Password = "";
		}
	}
	
	public void onAutoconnectCheckboxClick(View view)
	{
		_passwordET.setEnabled(_autoconnectCB.isChecked());
	}
	
	/**
	 * Валидация полей
	 */
	private boolean validate()
	{
		boolean hasErrors = false;
		
		String name = _nameET.getText().toString(); 
		if (name.length() == 0)
		{
			_nameET.setError("Наименование не задано");
			hasErrors = true;
		}
		else if (_configManager.nameAlreadyExists(name, _index))
		{
			_nameET.setError("БД с таким именем уже существует");
			hasErrors = true;
		}
		else
		{
			_nameET.setError(null);
		}
		
		String serverUrl = _serverUrlET.getText().toString();
		if (serverUrl.length() == 0)
		{
			_serverUrlET.setError("Адрес сервера не задан");
			hasErrors = true;
		}
		else if (!URLUtil.isHttpUrl(DatabaseSettings.formatServerAddresss(serverUrl)))
		{
			_serverUrlET.setError("Некорректный адрес сервера");
			hasErrors = true;
		}
		else
		{
			_serverUrlET.setError(null);
		}
				
		return !hasErrors;
	}
	
	/**
	 * Сохранить изменения
	 */
	public void onSaveButtonClick(View view)
	{
		// валидация полей формы
		if (!validate())
			return;
		
		// запомним старый адрес сервера
		String newServerUrl = _serverUrlET.getText().toString();
		
		// если эта настройка не вновь созданная и адрес сервера был изменен
		if (_index != -1 && !_dbSettings.ServerUrl.equals(newServerUrl))
		{			
			MessageBox.show(
					this, 
					"Настройка БД",
					"Адрес сервера был изменен. При сохранении БД будет очищена. Продолжить?", 
					MessageBox.QUESTION,
					true,
					new DialogInterface.OnClickListener()
		            {
		                public void onClick(DialogInterface dialog, int whichButton) 
		                {
		                	// считываем значения из полей формы в настройку БД
		            		readDataFromView();
		            		
		                	new LongOperation(
		                			DBSettingsActivity.this, 
		            				"Сброс БД...",
		            				new LongOperation.OperationHandler() 
		            				{
		            					@Override
		            					public void processResult(Object result) 
		            					{
		            						// сохраняем конфиг
		            						_configManager.save();
		            						// выходим
		            						DBSettingsActivity.this.finish();
		            					}

		            					@Override
		            					public Object execute() 
		            					{
		            						// удаляем весь кэш этой БД
		            						_cacheManager.remove(_dbSettings.Id.toString());
		            						return null;
		            					}
		            				}).run();
		                }
		            },
		            null);
			
			return;
		}

		// считываем значения из полей формы в настройку БД
		readDataFromView();
		
		// если это новая БД
		if (_index == -1)
			// то добавляем ее
			_configManager.getDatabaseSettings().add(_dbSettings);

		// сохраняем конфигурацию
		_configManager.save();
		
		// выходим
		finish();
	}
	
	/**
	 * Удалить БД
	 */
	public void onDeleteButtonClick(View view)
	{
		MessageBox.show(
				this, 
				"Настройка БД",
				"Удалить настройку БД?", 
				MessageBox.QUESTION,
				true,
				new DialogInterface.OnClickListener()
	            {
	                public void onClick(DialogInterface dialog, int whichButton) 
	                {
	                	new LongOperation(
	                			DBSettingsActivity.this, 
	            				"Удаление БД...",
	            				new LongOperation.OperationHandler() 
	            				{
	            					@Override
	            					public void processResult(Object result) 
	            					{
	            						DBSettingsActivity.this.finish();
	            					}

	            					@Override
	            					public Object execute() 
	            					{
	            						// удаляем весь кэш этой БД
	            						_cacheManager.remove(_dbSettings.Id.toString());
	            						// удаляем настройку этой БД
	            	                	_configManager.getDatabaseSettings().remove(_index);
	            	                	_configManager.save();
	            	                	
	            						return null;
	            					}
	            				}).run();
	                }
	            },
	            null);
	}
	
	/**
	 * Отмена изменений
	 */
	public void onCancelButtonClick(View view)
	{
		finish();
	}
	
	/**
	 * Очистить БД
	 */
	public void onClearButtonClick(View view)
	{
		MessageBox.show(
				this, 
				"Настройка БД", 
				"Очистить БД?", 
				MessageBox.QUESTION,
				true,
				new DialogInterface.OnClickListener()
	            {
	                public void onClick(DialogInterface dialog, int whichButton) 
	                {	            		
	                	new LongOperation(
	                			DBSettingsActivity.this, 
	            				"Сброс БД...",
	            				new LongOperation.OperationHandler() 
	            				{
	            					@Override
	            					public void processResult(Object result) 
	            					{
	            						MessageBox.showToast("БД очищена");
	            					}

	            					@Override
	            					public Object execute() 
	            					{
	            						// удаляем весь кэш этой БД
	            						_cacheManager.remove(_dbSettings.Id.toString());
	            						return null;
	            					}
	            				}).run();
	                }
	            },
	            null);
	}
}
