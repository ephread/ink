using Ink.LanguageServerProtocol.Workspace.Interfaces;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface ICompilerHostFactory
    {
        ICompilerHost CreateCompilerHost(IWorkspaceFileHandler fileHandler);
    }
}
