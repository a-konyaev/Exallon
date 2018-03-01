package ru.exallon.utils;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

import ru.exallon.Application;

public class PrecessHelper 
{
	/**
	 * Выполнить команду безопасно (т.е. не будет исключения, даже если команда выполнилась с ошибкой)
	 */
	public static void execSafe(String cmd)
	{
		try 
		{
			exec(cmd);
		} 
		catch (Exception e) {}
	}
	
	/**
	 * Выполнить команду
	 */
	public static void exec(String cmd) throws Exception
	{
		Process process = null;
		try
		{
			String[] cmdArr = {"/system/bin/sh", "-c", cmd};
			process = Runtime.getRuntime().exec(cmdArr);
			if (process.waitFor() == 0)
				return;
			
			BufferedReader reader = new BufferedReader(new InputStreamReader(process.getErrorStream()));
			StringBuilder total = new StringBuilder();
			String line;
			while ((line = reader.readLine()) != null) 
			{
				total.append(line);
			}
			
			String errMsg = total.toString();
			if (errMsg.length() == 0)
				return;
			
			throw new Exception(errMsg);
		}
		catch (Exception ex)
		{
			Application.logError(null, "Command '%s' failed: %s", cmd, ex.getMessage());
			throw new Exception("Execution failed", ex);
		}
		finally
		{
			if (process != null)
			{
				try 
				{
					process.getInputStream().close();
				} 
				catch (IOException e) {}
				
				try 
				{
					process.getOutputStream().close();
				} 
				catch (IOException e) {}
				
				try 
				{
					process.getErrorStream().close();
				} 
				catch (IOException e) {}
				
				process.destroy();
			}
		}
	}
}
