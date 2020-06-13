using System;
using Ink.LanguageServerProtocol.Helpers;
using Ink.LanguageServerProtocol.Models;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace
{
    public class LanguageServerEnvironment: ILanguageServerEnvironment
    {
        public Uri RootUri { get; private set; }

        public LanguageServerEnvironment()
        {

        }

        public void SetEnvironment(InitializeParams initializeParams)
        {
            RootUri = UriHelper.FromClientUri(initializeParams.RootUri);
        }
    }
}
