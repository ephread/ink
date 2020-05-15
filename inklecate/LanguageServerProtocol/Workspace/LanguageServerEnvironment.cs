using System;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace
{
    // Store environment provided through intialize event
    public class LanguageServerEnvironment: ILanguageServerEnvironment
    {
        public Uri RootUri { get; }

        public LanguageServerEnvironment(InitializeParams initializeParams)
        {
            RootUri = initializeParams.RootUri;
        }
    }
}
