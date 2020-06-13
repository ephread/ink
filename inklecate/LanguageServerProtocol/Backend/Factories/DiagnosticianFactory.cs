using System;
using Ink.LanguageServerProtocol.Backend;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol
{
    class DiagnosticianFactory: IDiagnosticianFactory {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IWorkspaceFileHandlerFactory _fileHandlerFactory;
        private readonly ILanguageServerConnection _connection;
        private readonly IVirtualWorkspaceManager _workspace;

        public DiagnosticianFactory(
            ILoggerFactory loggerFactory,
            IWorkspaceFileHandlerFactory fileHandlerFactory,
            ILanguageServerConnection connection,
            IVirtualWorkspaceManager workspace
        )
        {
            _loggerFactory = loggerFactory;
            _fileHandlerFactory = fileHandlerFactory;
            _connection = connection;
            _workspace = workspace;
        }

        public IDiagnostician CreateDiagnostician(Uri scopeUri)
        {
            var logger = _loggerFactory.CreateLogger<Diagnostician>();
            var fileHandler = _fileHandlerFactory.CreateFileHandler(scopeUri);

            return new Diagnostician(logger, _connection, _workspace, fileHandler);
        }
    }
}
