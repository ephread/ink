using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Ink.LanguageServerProtocol.Backend.Interfaces;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface IVirtualWorkspaceManager
    {
        Uri Uri { get; }

        TextDocumentItem GetTextDocument(Uri uri);
        void UpdateContentOfTextDocument(Uri uri, String text);
        void SetTextDocument(Uri uri, TextDocumentItem document);
        void RemoveTextDocument(Uri uri);

        ICompilationResult GetCompilationResult(Uri uri);
        void SetCompilationResult(Uri uri, ICompilationResult result);

        Uri ResolvePath(string path);
        Uri ResolvePath(string path, Uri mainDirectoryUri);
    }
}
