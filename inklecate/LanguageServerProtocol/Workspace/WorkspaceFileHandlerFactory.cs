using System;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol
{
    class WorkspaceFileHandlerFactory: IWorkspaceFileHandlerFactory {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILanguageServerEnvironment _environment;
        private readonly ILanguageServerConnection _connection;
        private readonly IVirtualWorkspaceManager _workspace;

        public WorkspaceFileHandlerFactory(
            ILoggerFactory loggerFactory,
            ILanguageServerEnvironment environment,
            ILanguageServerConnection connection,
            IVirtualWorkspaceManager workspace
        )
        {
            _loggerFactory = loggerFactory;
            _environment = environment;
            _connection = connection;
            _workspace = workspace;
        }

        public IWorkspaceFileHandler CreateFileHandler(Uri scopeUri)
        {
            var logger = _loggerFactory.CreateLogger<WorkspaceFileHandler>();
            return new WorkspaceFileHandler(
                logger,
                _environment.Copy(),
                _connection,
                _workspace.ReadOnlyCopy(),
                new Uri(scopeUri.ToString()));
        }
    }
}
