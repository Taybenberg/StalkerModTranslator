using System;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Google.Cloud.Translation.V2;

namespace StalkerModTranslator
{
    class GoogleTranslator : ITranslator, IDisposable
    {
        TranslationClient translationClient = TranslationClient.Create();

        public string Translate(string text, string langFrom, string langTo)
        {
            return translationClient.TranslateText(text, langTo, langFrom).TranslatedText;
        }

        public void Dispose()
        {
            translationClient.Dispose();
        }
    }

    class GoogleTranslatorAlternative : ITranslator, IDisposable
    {
        WebClient webClient = new WebClient
        {
            Encoding = Encoding.UTF8,
            UseDefaultCredentials = true,
        };

        public string Translate(string text, string langFrom, string langTo)
        {

            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={langFrom}&tl={langTo}&dt=t&q={WebUtility.UrlEncode(text)}";

            var result = webClient.DownloadString(url);

            JArray a = JArray.Parse(result);

            string str = string.Empty;

            foreach (var v in a[0])
                str += v[0].ToString();

            return str;
        }

        public void Dispose()
        {
            webClient.Dispose();
        }
    }
}
