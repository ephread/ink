using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface IVirtualWorkspaceManager
    {
        public TextDocumentItem GetTextDocument(Uri uri);
        public void UpdateContentOfTextDocument(Uri uri, String text);
        public void SetTextDocument(Uri uri, TextDocumentItem document);
        public void RemoveTextDocument(Uri uri);

        public void LoadDocumentContent(Uri uri);
        public Uri GetMainDocument(Uri scopeUri);
    }
}
