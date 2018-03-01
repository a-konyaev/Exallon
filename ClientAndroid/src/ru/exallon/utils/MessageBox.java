package ru.exallon.utils;

import ru.exallon.Application;
import ru.exallon.R;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.widget.Toast;

public class MessageBox 
{
	public static final int INFO = 0;
	public static final int QUESTION = 1;
	public static final int WARNING = 2;
	public static final int ERROR = 3;
	
	/**
	 * Показать сообщение
	 * @param context
	 * @param title			заголовок
	 * @param message		текст сообщения
	 * @param type			тип сообщения
	 */
	public static void show(
			Context context,
			String title, 
			String message, 
			int type)
	{
		show(context, title, message, type, false, null, null);
	}
	
	/**
	 * Показать сообщение
	 * @param context
	 * @param title			заголовок
	 * @param message		текст сообщения
	 * @param type			тип сообщения
	 * @param showCancel	отображать кнопку Отмена
	 * @param okClicked		обработчик события нажатия кнопки ОК
	 * @param cancelClicked	обработчик события нажатия кнопки Отмена
	 */
	public static void show(
			Context context,
			String title, 
			String message, 
			int type, 
			boolean showCancel, 
			DialogInterface.OnClickListener okClicked,
			DialogInterface.OnClickListener cancelClicked)
	{
		AlertDialog.Builder builder = new AlertDialog.Builder(context)
			.setTitle(title)
	        .setMessage(message)
	        .setPositiveButton("OK", okClicked);
		
		switch (type)
		{
		case INFO:
			builder.setIcon(R.drawable.msg_box_info);
			break;
		case QUESTION:
			builder.setIcon(R.drawable.msg_box_question);
			break;
		case WARNING:
			builder.setIcon(R.drawable.msg_box_warning);
			break;
		case ERROR:
			builder.setIcon(R.drawable.msg_box_error);
			break;
		}
		
		if (showCancel)
		{
			builder.setNegativeButton("Отмена", cancelClicked);
		}
		else
		{
			builder.setCancelable(false);
		}
		
		builder.show();
	}
	
	public static void showToast(String message)
	{
		Toast.makeText(Application.getContext(), message, Toast.LENGTH_SHORT).show();
	}
}
