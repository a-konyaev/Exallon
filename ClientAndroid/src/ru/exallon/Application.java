package ru.exallon;

import java.text.SimpleDateFormat;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.util.Log;

public class Application extends android.app.Application 
{
	public static String LOG_TAG = "Exallon";
	
	private static Application s_instance;
	
	public Application()
	{
		s_instance = this;
	}
	
	public static Context getContext()
	{
		return s_instance.getApplicationContext();
	}
	
	private static String _version;
	private static String _versionDate;
	
	/**
	 * Версия приложения
	 */
	public static String getVersion()
	{
		if (_version != null)
			return _version;
		
		try 
		{
			PackageManager pm = s_instance.getPackageManager();
			String app_pn = s_instance.getPackageName();
			_version = pm.getPackageInfo(app_pn, 0).versionName;
			return _version;
		} 
		catch (Exception e) 
		{
			Log.e(LOG_TAG, "getVersionName failed", e);
			return "1.0";
		}
	}
	
	/**
	 * Дата выпуска данной версии приложения
	 */
	public static String getReleaseDate()
	{
		if (_versionDate != null)
			return _versionDate;
		
		try 
		{
			PackageManager pm = s_instance.getPackageManager();
			String app_pn = s_instance.getPackageName();
			ApplicationInfo ai = pm.getApplicationInfo(app_pn, 0);
			
			ZipFile zf = new ZipFile(ai.sourceDir);
			ZipEntry ze = zf.getEntry("classes.dex");
			
			_versionDate = new SimpleDateFormat("dd.MM.yyyy").format(ze.getTime());
			return _versionDate;
		} 
		catch (Exception e) 
		{
			Log.e(LOG_TAG, "getReleaseDate failed", e);
			return "01.01.0100";
		}
	}	
	
	public static void logVerbose(String subLogTag, String format, Object... args)
	{
		log(Log.VERBOSE, subLogTag, format, args);
	}
	
	public static void logInfo(String subLogTag, String format, Object... args)
	{
		log(Log.INFO, subLogTag, format, args);
	}
	
	public static void logWarning(String subLogTag, String format, Object... args)
	{
		log(Log.WARN, subLogTag, format, args);
	}
	
	public static void logError(String subLogTag, String format, Object... args)
	{
		log(Log.ERROR, subLogTag, format, args);
	}
	
	public static void logError(String subLogTag, Throwable tr, String format, Object... args)
	{
		log(Log.ERROR, subLogTag, format + ": " + tr.toString(), args);
	}
	
	public static void log(int priority, String subLogTag, String format, Object... args)
	{
		if (!Log.isLoggable(LOG_TAG, priority))
			return;
		
		String msg = String.format(format, args);
		
		if (subLogTag != null)
		{
			StringBuilder sb = new StringBuilder(256);
			sb.append(subLogTag);
			sb.append(": ");
			sb.append(msg);
			msg = sb.toString();
		}
		
		Log.println(priority, LOG_TAG, msg);
	}
}
