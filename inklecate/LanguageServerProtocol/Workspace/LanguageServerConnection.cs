using Ink.LanguageServerProtocol.Workspace.Interfaces;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Ink.LanguageServerProtocol.Workspace
{
    /// <summary>
    /// Wrap connection objects (`Document`, `Client`, `Window` and `Workspace`)
    /// into an injectable singleton. All methods and properties on
    /// connection obects are considered thread safe.
    /// </summary>
    public class LanguageServerConnection: ILanguageServerConnection
    {
        private ILanguageServer _server;

        public LanguageServerConnection() { }

        public ILanguageServerDocument Document
        {
            get { return _server.Document; }
        }

        public ILanguageServerClient Client
        {
            get { return _server.Client; }
        }

        public ILanguageServerWindow Window
        {
            get { return _server.Window; }
        }

        public ILanguageServerWorkspace Workspace
        {
            get { return _server.Workspace; }
        }

        public void SetServer(ILanguageServer server)
        {
            _server = server;
        }
    }
}
