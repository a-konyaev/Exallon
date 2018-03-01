package ru.exallon.utils;

import android.app.ProgressDialog;
import android.content.Context;
import android.os.Handler;
import android.os.Message;

public class LongOperation 
{
	public interface OperationHandler
	{
		Object execute();
		void processResult(Object result);
	}
	
	private Context _context;
	private String _progressMessage;
	private OperationHandler _operation;
	private Object _result;
	
	public LongOperation(
			Context context,
			String progressMessage,
			OperationHandler operation)
	{
		_context = context;
		_progressMessage = progressMessage;
		_operation = operation;
		_result = null;
	}
	
	public void run()
	{
		// прогресс-диалог
    	final ProgressDialog dialog = new ProgressDialog(_context);
    	dialog.setMessage(_progressMessage);
    	dialog.setIndeterminate(true);
    	
    	// обработчик завершения загрузки данных
    	final Handler handler = new Handler() 
    	{
    		public void handleMessage(Message msg) 
    		{
    			// если отображали прогресс-диалог
    			if (dialog != null && dialog.isShowing())
    				// то скроем его
    				dialog.dismiss();
    				
    			// обработка результата
    			_operation.processResult(_result);
    			_result = null;
    		}
    	};
    	
    	// событие готовности загрузки данных
    	final AutoResetEvent getDataDone = new AutoResetEvent(false);
    	
    	// запускаем поток загрузки данных
    	new Thread() 
    	{  
    		public void run() 
    		{
    			// выполняем операцию
    			_result = _operation.execute();
    			// сообщаем, что выполнение завершено
    			getDataDone.set();
    			handler.sendEmptyMessage(0);
    		}
    	}.start();
    	
    	try 
    	{
    		// если в течение 1 сек данные так и не загрузились
			if (!getDataDone.waitOne(1000))
			{
				// то отображаем прогресс-диалог
				dialog.show();
			}
		} 
    	catch (InterruptedException e) 
    	{
		}	
	}
}
