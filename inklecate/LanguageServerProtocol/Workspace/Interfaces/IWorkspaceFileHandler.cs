using System;
using System.Threading.Tasks;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface IWorkspaceFileHandler: Ink.IFileHandler
    {
        string LoadDocumentContent(Uri uri);
        Task<Uri> GetMainDocument();
    }
}
