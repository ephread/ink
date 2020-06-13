using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    /// <summary>
    /// 1 - Wrap the Language Server connection elements (Document, Workspace,
    /// Window and Client) to make them available for injection without
    /// exposing the server instance.
    ///
    /// 2 - The Language Server is not yet created at the time of the services
    /// registration, although we need to make the connection object available
    /// for injection. This Singleton can be injected without waiting for
    /// the language server to be intantiated, as its properties can be updated
    /// at a later stage.
    /// </summary>
    public interface ILanguageServerConnection
    {
        ILanguageServerDocument Document { get; }
        ILanguageServerClient Client { get; }
        ILanguageServerWindow Window { get; }
        ILanguageServerWorkspace Workspace { get; }
    }
}
