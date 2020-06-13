using System;
using Newtonsoft.Json.Linq;

namespace Ink.LanguageServerProtocol.Models
{
    /// <summary>
    /// Configuration settings set on the client for ink.*
    ///
    /// Usually scoped at the file level (multi-root workspaces).
    /// </summary>
    public class InkConfiguration
    {
        public readonly string mainFilePath = null;

        public bool IsMainStoryDefined
        {
            get { return !string.IsNullOrWhiteSpace(mainFilePath); }
        }

        public InkConfiguration()
        {

        }

        public InkConfiguration(string mainFilePath)
        {
            this.mainFilePath = mainFilePath;
        }

        public InkConfiguration(InkConfiguration defaultConfiguration, JToken jToken)
        {
            if (jToken is JObject jObject)
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