using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface IVirtualWorkspaceManager: IReadOnlyVirtualWorkspaceManager
    {
        void UpdateContentOfTextDocument(Uri uri, String text);
        void SetTextDocument(Uri uri, TextDocumentItem document);
        void RemoveTextDocument(Uri uri);

        IReadOnlyVirtualWorkspaceManager ReadOnlyCopy();
    }
}
