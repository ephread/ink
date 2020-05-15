using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface ILanguageServerEnvironment
    {
        Uri RootUri { get; }
    }
}
