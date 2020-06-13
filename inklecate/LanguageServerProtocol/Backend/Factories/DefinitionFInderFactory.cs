using System;
using Ink.LanguageServerProtocol.Backend;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol
{
    class DefinitionFinderFactory: IDefinitionFinderFactory {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IWorkspaceFileHandlerFactory _fileHandlerFactory;
        private readonly IVirtualWorkspaceManager _workspace;

        public DefinitionFinderFactory(
            ILoggerFactory loggerFactory,
            IWorkspaceFileHandlerFactory fileHandlerFactory,
            IVirtualWorkspaceManager workspace
        )
        {
            _loggerFactory = loggerFactory;
            _fileHandlerFactory = fileHandlerFactory;
            _workspace = workspace;
        }

        public IDefinitionFinder CreateDefinitionFinder(Uri documentUri)
        {
            var logger = _loggerFactory.CreateLogger<DefinitionFinder>();
            var fileHandler = _fileHandlerFactory.CreateFileHandler(documentUri);

            return new DefinitionFinder(logger, _workspace, fileHandler, documentUri);
        }
    }
}
