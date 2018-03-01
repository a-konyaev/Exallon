using System.IO;
using System.Reflection;

namespace Exallon.Utils
{
    public class FileHelper
    {
        /// <summary>
        /// Корневая директория приложения
        /// </summary>
        public static string RootDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
