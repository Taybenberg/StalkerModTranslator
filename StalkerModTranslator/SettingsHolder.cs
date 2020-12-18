using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StalkerModTranslator
{
    class SettingsHolder
    {
        private string _sourceDir = null, _langFrom = null;
        private bool? _printAllStrings = null, _fixBadFiles = null;
        private int _sleepDelay = -1;

        public SettingsHolder()
        {
            Console.WriteLine(
               "Вітаємо у застосунку для українізації текстових файлів гри S.T.A.L.K.E.R.\n\n" +
               "Для початку роботи скопіюйте у теку з застосунком теку " +
               "\"text\" з текстовими xml-файлами (gamedata\\configs\\text)\n");
        }

        string getUserString(string request)
        {
            Console.WriteLine(request);
            return Console.ReadLine();
        }

        public string SourceDir
        {
            set { _sourceDir = value; }
            get 
            {
                if (_sourceDir is null)
                    _sourceDir = getUserString("\nВведіть назву теки з текстовими файлами, яка знаходиться всередині теки text (наприклад, \"eng\", чи \"rus\")");     
                return _sourceDir;
            }
        }

        public string LangFrom
        {
            set { _langFrom = value; }
            get
            {
                if (_langFrom is null)
                    _langFrom = getUserString("\nВведіть мовний код ISO 639-1 тієї мови, з якої здійснюється переклад(наприклад, \"en\", чи \"ru\")");
                return _langFrom;
            }
        }

        public bool? PrintAllStrings
        {
            set { _printAllStrings = value; }
            get
            {
                if (_printAllStrings is null)
                    _printAllStrings = getUserString("\nВиводити рядки, які перекладаються? y|n?") == "y";
                return _printAllStrings;
            }
        }

        public bool? FixBadFiles
        {
            set { _fixBadFiles = value; }
            get
            {
                if (_fixBadFiles is null)
                    _fixBadFiles = getUserString("\n\nСпробувати виправити файли з порушеною структурою? y|n?") == "y";
                return _fixBadFiles;
            }
        }

        public int SleepDelay
        {
            set { _sleepDelay = value; }
            get
            {
                if (_sleepDelay < 0)
                     Int32.TryParse(Console.ReadLine(), out _sleepDelay);
                return _sleepDelay;
            }
        }
    }
}
