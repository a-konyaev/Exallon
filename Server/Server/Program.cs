using System;

namespace Exallon
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("Exallon server starting...");
            try
            {
                var app = new Application();

                app.Logger.Info("Exallon server started. Press <Enter> to stop");
                Console.ReadLine();

                // освобождаем ресурсы
                app.Dispose();
            }
            catch
            {
                Console.WriteLine("Create application FAILED! Press <Enter> to exit");
                Console.ReadLine();
            }
#else
            System.ServiceProcess.ServiceBase.Run(new Service());
#endif
        }
    }
}
