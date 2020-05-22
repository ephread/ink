using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface IReadOnlyVirtualWorkspaceManager
    {
        TextDocumentItem GetTextDocument(Uri uri);

        Uri ResolvePath(string path);
        Uri ResolvePath(string path, Uri mainDirectoryUri);
    }
}
