package ru.exallon;

import ru.exallon.R;
import android.app.Activity;
import android.os.Bundle;
import android.text.Html;
import android.text.method.LinkMovementMethod;
import android.widget.TextView;

public class HelpActivity extends Activity 
{
	@Override
	public void onCreate(Bundle savedInstanceState) 
	{
	    super.onCreate(savedInstanceState);
	    setContentView(R.layout.help);
	    
	    TextView view = (TextView)findViewById(R.id.help_text);
	    view.setMovementMethod(LinkMovementMethod.getInstance());
	    
	    view.setText(Html.fromHtml(getString(R.string.help)));
	}
}
