using System;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface IWorkspaceFileHandlerFactory
    {
        IWorkspaceFileHandler CreateFileHandler(Uri scopeUri);
    }
}
