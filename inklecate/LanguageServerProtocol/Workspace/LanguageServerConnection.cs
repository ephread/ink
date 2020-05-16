using Ink.LanguageServerProtocol.Workspace.Interfaces;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Ink.LanguageServerProtocol.Workspace
{
    // Wrap connection objects (Document, Workspace, Window and Client)
    // into an injectable singleton.
    public class LanguageServerConnection: ILanguageServerConnection
    {
        private ILanguageServer _server;

        public LanguageServerConnection() {

        }

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
