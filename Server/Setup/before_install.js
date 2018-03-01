// останавливаем и удаляем предыдущую версию сервиса Exallon
_shell = new ActiveXObject("WScript.Shell");
_shell.Run("cmd /c \"sc stop Exallon\"", 1, true);
_shell.Run("cmd /c \"sc delete Exallon\"", 1, true);
