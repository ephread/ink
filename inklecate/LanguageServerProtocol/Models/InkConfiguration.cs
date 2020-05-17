using System;
using Newtonsoft.Json.Linq;

namespace Ink.LanguageServerProtocol.Models
{
    public class InkConfiguration
    {
        public readonly string mainFilePath = null;

        public bool IsMainStoryDefined
        {
            get { return !String.IsNullOrWhiteSpace(mainFilePath); }
        }

        public InkConfiguration()
        {

        }

        public InkConfiguration(string mainFilePath, string inklecatePath)
        {
            this.mainFilePath = mainFilePath;
        }

        public InkConfiguration(InkConfiguration defaultConfiguration, JToken jToken)
        {
            var jObject = jToken as JObject;
            if (jObject != null)
            {
                if (jObject.TryGetValue("languageServer", out JToken configJToken))
                {
                    mainFilePath = configJToken.Value<string>("mainFilePath") ?? defaultConfiguration.mainFilePath;

                    return;
                }
            }

            mainFilePath = defaultConfiguration.mainFilePath;
        }

        public static InkConfiguration Default
        {
            get { return new InkConfiguration(); }
        }
    }
}