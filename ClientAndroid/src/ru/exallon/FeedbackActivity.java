package ru.exallon;

import java.io.File;
import java.text.SimpleDateFormat;
import java.util.Date;

import ru.exallon.utils.LongOperation;
import ru.exallon.utils.MessageBox;
import ru.exallon.utils.PrecessHelper;
import ru.exallon.R;
import android.app.Activity;
import android.content.ContextWrapper;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.widget.CheckBox;
import android.widget.EditText;

public class FeedbackActivity extends Activity 
{
	public static String SUB_LOG_TAG = "FeedbackActivity";
	
	@Override
	public void onCreate(Bundle savedInstanceState) 
	{
	    super.onCreate(savedInstanceState);
	    setContentView(R.layout.feedback);
	}
	
	/**
     * Обработка нажатия кнопки "Отправить"
     */
    public void onSendButtonClick(View view) 
	{
    	String body = getEmailBody();
    	boolean attachLogs = ((CheckBox)this.findViewById(R.id.feedback_attach_logs)).isChecked();
    	
    	Application.logInfo(SUB_LOG_TAG, 
    			"Feedback sending: body='%s'; attachLogs='%s'",
    			body, attachLogs);
    	
    	if (body.length() == 0 && !attachLogs)
    	{
    		MessageBox.show(
    				this, 
    				"Не введены данные",
    				"Введите текст сообщения или установите флаг 'Прикрепить логи'", 
    				MessageBox.WARNING);
    		return;
    	}
    	
		final Intent emailIntent = new Intent(android.content.Intent.ACTION_SEND);
		emailIntent.setType("plain/text");
		emailIntent.putExtra(Intent.EXTRA_EMAIL, new String[] { "info@exallon.ru" });
		emailIntent.putExtra(Intent.EXTRA_SUBJECT, getEmailSubject());
		emailIntent.putExtra(Intent.EXTRA_TEXT, body);
		
		if (!attachLogs)
		{
			startActivity(Intent.createChooser(emailIntent, "Отправка отзыва..."));
			finish();
		}

		String filesDirPath = new ContextWrapper(this).getFilesDir().getPath();

		// удалим старые логи, если есть
		PrecessHelper.execSafe(String.format("rm %s/log*.*", filesDirPath));

		// сформируем путь к новому файлу с логами
		String timestamp = new SimpleDateFormat("yyyyMMddHHmmss").format(new Date());
		final String logFilePath = String.format("%s/log_%s_%s.txt",
				filesDirPath, Application.getVersion(), timestamp);

		// сохраняем логи в файл
		new LongOperation(this, "Сохранение логов...",
				new LongOperation.OperationHandler() 
				{
					@Override
					public void processResult(Object result) 
					{
						if ((Boolean) result) 
						{
							emailIntent.putExtra(Intent.EXTRA_STREAM, Uri.fromFile(new File(logFilePath)));
							startActivity(Intent.createChooser(emailIntent, "Отправка отзыва..."));
							finish();
						} 
						else 
						{
							MessageBox.showToast("Ошибка сохранения логов");
						}
					}

					@Override
					public Object execute() 
					{
						try 
						{
							String cmd = String.format(
									"logcat -d %s:V ActivityManager:I *:E >%s",
									Application.LOG_TAG, logFilePath);
							PrecessHelper.exec(cmd);
							return true;
						} 
						catch (Exception ex) 
						{
							Application.logError(SUB_LOG_TAG, ex, "Save logs failed");
							return false;
						}
					}
				}).run();
	}
 
    private String getEmailSubject()
    {
    	return String.format("Exallon Feedback (%s-%s)", 
    			Application.getVersion(), Application.getReleaseDate());
    }
    
    private String getEmailBody()
    {
    	EditText msgView = (EditText)this.findViewById(R.id.feedback_message);
    	return msgView.getText().toString();
    }
    
    /**
     * Обработка нажатия кнопки "Отмена"
     */
    public void onCancelButtonClick(View view) 
	{
    	finish();
	}
}
