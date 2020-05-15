using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface ILanguageServerConnection
    {
        ILanguageServerDocument Document { get; }
        ILanguageServerClient Client { get; }
        ILanguageServerWindow Window { get; }
        ILanguageServerWorkspace Workspace { get; }
    }
}
