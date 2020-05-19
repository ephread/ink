using System;
using Ink.LanguageServerProtocol.Helpers;
using Ink.LanguageServerProtocol.Models;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace
{
    // Store environment provided through intialize event
    public class LanguageServerEnvironment: ILanguageServerEnvironment
    {
        public Uri RootUri { get; private set; }
        public InkConfiguration Configuration { get; }

        public LanguageServerEnvironment()
        {
            Configuration = new InkConfiguration();
        }

        public void SetEnvironment(InitializeParams initializeParams)
        {
            RootUri = UriHelper.fromClientUri(initializeParams.RootUri);
        }
    }
}
