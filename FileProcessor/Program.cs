using System;

namespace FileProcessor
{
    class Program
    {
        
        static string InputFolderPath { get; set; }
        static string OutputFile { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl+C to Exit");

            Initializer();

            FileProcesor.ProcessFile(InputFolderPath, OutputFile);

        }


        static private void Initializer()
        {
            InputFolderPath = ConfigReader.GetInputFolderPath();
            OutputFile = ConfigReader.GetOutputFile();
        }
    }
}
