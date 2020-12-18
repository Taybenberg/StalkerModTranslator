using System;
using System.Text;

namespace StalkerModTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.OutputEncoding = Encoding.GetEncoding(1251);

            Console.ForegroundColor = ConsoleColor.White;

            SettingsHolder settings = new();

            if (args.Length > 0)
            {
                settings.SourceDir = args[0];
                settings.LangFrom = args[1];
                settings.PrintAllStrings = args[2] == "y";
                settings.FixBadFiles = args[3] == "y";
                settings.SleepDelay = int.Parse(args[4]);
            }

            XML_Processor processor = new(new GoogleTranslator(), settings);
        }
    }
}