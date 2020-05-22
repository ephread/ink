using System;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol.Backend
{
    class CompilerHostFactory: ICompilerHostFactory {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILanguageServerConnection _connection;

        public CompilerHostFactory(
            ILoggerFactory loggerFactory,
            ILanguageServerConnection connection
        )
        {
            _loggerFactory = loggerFactory;
            _connection = connection;
        }

        public ICompilerHost CreateCompilerHost(IWorkspaceFileHandler fileHandler)
        {
            var logger = _loggerFactory.CreateLogger<CompilerHost>();
            return new CompilerHost(logger, _connection, fileHandler);
        }
    }
}
