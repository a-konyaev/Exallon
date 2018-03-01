// запускаем установленную версию сервиса Exallon
_shell = new ActiveXObject("WScript.Shell");
_shell.Run("cmd /c \"sc start Exallon\"", 1, true);
