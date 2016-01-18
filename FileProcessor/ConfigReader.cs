using System.Configuration;

namespace FileProcessor
{
    /// <summary>
    /// Read the app.config settings
    /// </summary>
    static class ConfigReader
    {
       
        static public string GetInputFolderPath()
        {
            if (ConfigurationManager.AppSettings.Count == 0)
            {
                //if not settings return default
                return @"c:\temp";
            }
            return ConfigurationManager.AppSettings["InputFolderPath"];
        }

        static public string GetOutputFile()
        {
            if (ConfigurationManager.AppSettings.Count == 0)
            {
                ////if settings not set, return default
                return @"c:\output.dat";
            }
            return ConfigurationManager.AppSettings["OutputFile"];
        }
    }
}
