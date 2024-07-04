using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend
{
    public static class Helper
    {

        public static void WriteLineCyan(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteIntoConsoleAndLog(text);
            Console.ResetColor();
        }

        public static void WriteLineRed(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteIntoConsoleAndLog(text);
            Console.ResetColor();
        }

        public static void WriteIntoConsoleAndLog(string? text)
        {
            if (text != null)
            {
                Console.WriteLine(text);
                WriteToLogFile(text);
            }
        }

        public static void WriteToLogFile(string text)
        {
            try
            {
                string logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logFolderPath);

                string logFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                string logFilePath = Path.Combine(logFolderPath, logFileName);

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(text);
                }
            }
            catch (Exception _)
            {

            }

        }
    }
}
