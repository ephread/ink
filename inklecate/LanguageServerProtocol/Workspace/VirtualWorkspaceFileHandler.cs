using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol.Workspace
{
    // Will resolve file names and then ask the virtual workspace to
    // either read files from the disk or return in-memory buffers.
    public class VirtualWorkspaceFileHandler: Ink.IFileHandler
    {
        private readonly ILogger<VirtualWorkspaceFileHandler> _logger;
        private readonly IVirtualWorkspaceManager _virtualWorkspace;

        public VirtualWorkspaceFileHandler(
            ILogger<VirtualWorkspaceFileHandler> logger,
            IVirtualWorkspaceManager virtualWorkspace)
        {
            _logger = logger;
            _virtualWorkspace = virtualWorkspace;
        }

        public string ResolveInkFilename (string includeName) {
            return "";
        }

        public string LoadInkFileContents (string fullFilename) {
            return "";
        }
    }
}
