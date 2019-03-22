using System;
using System.IO;
using System.Text;

namespace StalkerModTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding(1251);

            Console.ForegroundColor = ConsoleColor.White;

            if (args.Length > 0)
                new StalkerTranslator(args[0], args[1], args[2], args[3], args[4]);
            else
                new StalkerTranslator();
        }
    }
}
