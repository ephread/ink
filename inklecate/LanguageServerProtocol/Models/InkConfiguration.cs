using System;
using Newtonsoft.Json.Linq;

namespace Ink.LanguageServerProtocol.Models
{
    public class InkConfiguration
    {
        public readonly string mainStoryPath = null;
        public readonly string inklecatePath = "inklecate";
        public readonly bool runThroughMono = false;

        public bool IsMainStoryDefined
        {
            get { return !String.IsNullOrWhiteSpace(mainStoryPath); }
        }

        public InkConfiguration()
        {

        }

        public InkConfiguration(string mainStoryPath, string inklecatePath, bool runThroughMono)
        {
            this.mainStoryPath = mainStoryPath;
            this.inklecatePath = inklecatePath;
            this.runThroughMono = runThroughMono;
        }

        public InkConfiguration(InkConfiguration defaultConfiguration, JToken jToken)
        {
            mainStoryPath = jToken.Value<string>("mainStoryPath") ?? defaultConfiguration.mainStoryPath;
            inklecatePath = jToken.Value<string>("inklecatePath") ?? defaultConfiguration.inklecatePath;
            runThroughMono = jToken.Value<bool?>("runThroughMono") ?? defaultConfiguration.runThroughMono;
        }

        public static InkConfiguration Default
        {
            get { return new InkConfiguration(); }
        }
    }
}