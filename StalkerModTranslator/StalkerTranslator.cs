using System;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Xml.Serialization;
using Google.Cloud.Translation.V2;
using Newtonsoft.Json.Linq;

namespace StalkerModTranslator
{
    public class StalkerTranslator
    {
        bool mark = false;
        bool fix = false;
        int delay = 0;

        List<string> badFiles = new List<string>();

        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        TranslationClient translationClient = TranslationClient.Create();

        WebClient webClient = new WebClient
        {
            Encoding = Encoding.UTF8,
            UseDefaultCredentials = true,
        };

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

        public string Translate(string text, string langFrom, string langTo)
        {
            //Альтернативний варіант
            /*
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={langFrom}&tl={langTo}&dt=t&q={WebUtility.UrlEncode(text)}";

            var result = webClient.DownloadString(url);

            JArray a = JArray.Parse(result);

            string str = string.Empty;

            foreach (var v in a[0])
                str += v[0].ToString();

            return str;*/

            return translationClient.TranslateText(text, langTo, langFrom).TranslatedText;
        }

        void TranslateFile(string sourcePath, string destPath, string langFrom, bool log = true)
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

                if (mark)
                {
                    for (int i = 0; i < st.String.Count; i++)
                    {
                        Console.WriteLine(st.String[i].Text);

                        st.String[i].Text = Translate(st.String[i].Text, langFrom, "uk");

                        Console.WriteLine(st.String[i].Text);

                        Thread.Sleep(delay);
                    }
                }
                else
                {
                    for (int i = 0; i < st.String.Count; i++)
                    {
                        st.String[i].Text = Translate(st.String[i].Text, "en", "uk");

                        Thread.Sleep(delay);
                    }
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

        public StalkerTranslator(string sourceDir = null, string langFrom = null, string printAllStrings = null, string fixBadFiles = null, string sleepDelay = null)
        {
            ns.Add("", "");

            if (!Directory.Exists("text\\ukr"))
                Directory.CreateDirectory("text\\ukr");

            Console.WriteLine(
                "Вітаємо у застосунку для українізації текстових файлів гри S.T.A.L.K.E.R.\n\n" +
                "Для початку роботи скопіюйте у теку з застосунком теку " +
                "\"text\" з текстовими xml-файлами (gamedata\\configs\\text)\n");

            if (sourceDir == null)
            {
                Console.WriteLine("\nВведіть назву теки з текстовими файлами, яка знаходиться всередині теки text (наприклад, \"eng\", чи \"rus\")");
                sourceDir = Console.ReadLine();
            }

            if (langFrom == null)
            {
                Console.WriteLine("\nВведіть мовний код ISO 639-1 тієї мови, з якої здійснюється переклад (наприклад, \"en\", чи \"ru\")");
                langFrom = Console.ReadLine();
            }

            if (sleepDelay == null)
            {
                Console.WriteLine("\nВведіть затримку між перекладом рядків (у мілісекундах, потрібно для дуже великих файлів)");
                Int32.TryParse(Console.ReadLine(), out delay);
            }
            else
                Int32.TryParse(sleepDelay, out delay);

            if (printAllStrings == null)
            {
                Console.WriteLine("\nВиводити рядки, які перекладаються? y|n?");
                mark = "y" == Console.ReadLine();
            }
            else
                mark = "y" == printAllStrings;

            foreach (var path in Directory.GetFiles($"text\\{sourceDir}"))
                if (!File.Exists(path.Replace($"\\{sourceDir}\\", "\\ukr\\")))
                    TranslateFile(path, path.Replace($"\\{sourceDir}\\", "\\ukr\\"), langFrom);

            if (fixBadFiles == null)
            {
                Console.WriteLine("\n\nЗавершено!\nФайли, у яких порушена структура:");
                foreach (var b in badFiles)
                    Console.WriteLine(b);

                Console.WriteLine("\n\nСпробувати виправити ці файли? y|n?");
                fix = "y" == Console.ReadLine();
            }
            else
                fix = "y" == fixBadFiles;

            if (fix)
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

                        TranslateFile(tmpFile, file.Replace($"\\{sourceDir}\\", "\\ukr\\"), langFrom, false);

                        File.Delete(tmpFile);

                        xmlFile = File.ReadAllText(file.Replace($"\\{sourceDir}\\", "\\ukr\\"), Encoding.GetEncoding(1251));

                        xmlFile = xmlFile.Replace(end, string.Empty);
                        xmlFile = xmlFile.Remove(0, begin.Length);

                        File.WriteAllText(file.Replace($"\\{sourceDir}\\", "\\ukr\\"), xmlFile, Encoding.GetEncoding(1251));
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
}