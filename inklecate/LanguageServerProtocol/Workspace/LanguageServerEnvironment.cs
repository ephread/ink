using System;
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

        }

        private LanguageServerEnvironment(LanguageServerEnvironment environment)
        {
            RootUri = environment.RootUri;
            Configuration = environment.Configuration;
        }

        public void SetEnvironment(InitializeParams initializeParams)
        {
            RootUri = initializeParams.RootUri;
        }

        public ILanguageServerEnvironment Copy() {
            return new LanguageServerEnvironment(this);
        }
    }
}
