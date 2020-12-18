using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StalkerModTranslator
{
    class XML_Processor : IFileProcessor
    {
        ITranslator translator;

        SettingsHolder settings;

        List<string> badFiles = new();

        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        [XmlRoot(ElementName = "string")]
        public class String
        {
            [XmlElement(ElementName = "text")]
            public string Text { get; set; }
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }
        }

        [XmlRoot(ElementName = "string_table")]
        public class string_table
        {
            [XmlElement(ElementName = "string")]
            public List<String> String { get; set; }
        }

        public XML_Processor(ITranslator translator, SettingsHolder settings)
        {
            ns.Add("", "");

            this.translator = translator;

            this.settings = settings;

            if (!Directory.Exists("text\\ukr"))
                Directory.CreateDirectory("text\\ukr");

            foreach (var path in Directory.GetFiles($"text\\{settings.SourceDir}"))
                if (!File.Exists(path.Replace($"\\{settings.SourceDir}\\", "\\ukr\\")))
                    TranslateFile(path, path.Replace($"\\{settings.SourceDir}\\", "\\ukr\\"), settings.LangFrom);

            Console.WriteLine("\n\nЗавершено!\nФайли, у яких порушена структура:");
            foreach (var b in badFiles)
                Console.WriteLine(b);

            if (settings.FixBadFiles ?? false)
                FixFiles();
        }

        public void TranslateFile(string sourcePath, string destPath, string langFrom, bool log = true)
        {
            try
            {
                string_table st = null;

                Console.WriteLine($"\nВхідний файл: {sourcePath}\n");

                XmlSerializer serializer = new XmlSerializer(typeof(string_table));

                using (XmlReader reader = XmlReader.Create(sourcePath))
                {
                    st = (string_table)serializer.Deserialize(reader);
                }

                for (int i = 0; i < st.String.Count; i++)
                {
                    if (settings.PrintAllStrings ?? false)
                        Console.WriteLine(st.String[i].Text);

                    st.String[i].Text = translator.Translate(st.String[i].Text, langFrom, "uk");

                    if (settings.PrintAllStrings ?? false)
                        Console.WriteLine(st.String[i].Text);

                    Thread.Sleep(settings.SleepDelay);
                }

                using (FileStream saveFile = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                {
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(saveFile, Encoding.GetEncoding("windows-1251")))
                    {
                        xmlTextWriter.Formatting = Formatting.Indented;
                        xmlTextWriter.Indentation = 4;

                        serializer.Serialize(xmlTextWriter, st, ns);

                        Console.WriteLine($"\nПерекладено!\nВихідний файл: {destPath}\n");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message + "\n");

                if (log)
                    badFiles.Add(sourcePath);
            }
        }

        public void FixFiles()
        {
            List<string> nonFixed = new List<string>();

            string begin = "<?xml version=\"1.0\" encoding=\"windows-1251\"?>\n<string_table>\n";
            string end = "\n</string_table>";

            foreach (var file in badFiles)
            {
                try
                {
                    string xmlFile = begin + File.ReadAllText(file, Encoding.GetEncoding(1251)) + end;

                    string tmpFile = Path.GetTempFileName();

                    File.WriteAllText(tmpFile, xmlFile, Encoding.GetEncoding(1251));

                    TranslateFile(tmpFile, file.Replace($"\\{settings.SourceDir}\\", "\\ukr\\"), settings.LangFrom, false);

                    File.Delete(tmpFile);

                    xmlFile = File.ReadAllText(file.Replace($"\\{settings.SourceDir}\\", "\\ukr\\"), Encoding.GetEncoding(1251));

                    xmlFile = xmlFile.Replace(end, string.Empty);
                    xmlFile = xmlFile.Remove(0, begin.Length);

                    File.WriteAllText(file.Replace($"\\{settings.SourceDir}\\", "\\ukr\\"), xmlFile, Encoding.GetEncoding(1251));
                }
                catch (Exception e)
                {
                    Console.WriteLine("\n" + e.Message + "\n");
                    nonFixed.Add(file);
                }
            }

            Console.WriteLine("\n\nЗавершено!\n");
            Console.WriteLine("Файли, які не вдалося виправити:");
            foreach (var n in nonFixed)
                Console.WriteLine(n);

            Console.ReadLine();
        }
    }
}
