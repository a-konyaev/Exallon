package ru.exallon;

import ru.exallon.R;
import android.app.Activity;
import android.os.Bundle;
import android.text.Html;
import android.text.method.LinkMovementMethod;
import android.widget.TextView;

/**
 * Страница "О программе"
 */
public class AboutActivity extends Activity 
{
	@Override
	public void onCreate(Bundle savedInstanceState) 
	{
	    super.onCreate(savedInstanceState);
	    setContentView(R.layout.about);
	    
	    TextView view = (TextView)findViewById(R.id.about_text);
	    view.setMovementMethod(LinkMovementMethod.getInstance());
	    view.setText(Html.fromHtml(
                "Версия: " + Application.getVersion() + "<br/>" +
                "Дата выпуска: " + Application.getReleaseDate() + "<br/><br/>" +
                "\u00A9 Exallon, 2012<br/>Все права защищены<br/>" +
                "Посетите наш сайт: <a href=\"http://www.exallon.ru\">www.exallon.ru</a>"));
	}
}
