using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface IVirtualWorkspaceManager
    {
        TextDocumentItem GetTextDocument(Uri uri);
        void UpdateContentOfTextDocument(Uri uri, String text);
        void SetTextDocument(Uri uri, TextDocumentItem document);
        void RemoveTextDocument(Uri uri);

        Uri GetUriFromAbsolutePath(string path);
        Uri GetUriFromRelativePath(Uri rootUri, string path);
    }
}
