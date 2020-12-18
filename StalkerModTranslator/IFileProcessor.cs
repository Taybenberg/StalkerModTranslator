using System.Collections.Generic;

namespace StalkerModTranslator
{
    interface IFileProcessor
    {
        void TranslateFile(string sourcePath, string destPath, string langFrom, bool log);

        void FixFiles();
    }
}