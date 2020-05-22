using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Extensions;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Ink.LanguageServerProtocol.Backend
{
    // Compile or run static analysis, then push diagnostics back to the client.
    public class DiagnosticManager: IDiagnosticManager
    {
        private readonly ILogger<DiagnosticManager> _logger;
        private readonly IWorkspaceFileHandlerFactory _fileHandlerFactory;
        private readonly ICompilerHostFactory _compilerHostFactory;

        public DiagnosticManager(
            ILogger<DiagnosticManager> logger,
            IWorkspaceFileHandlerFactory fileHandlerFactory,
            ICompilerHostFactory compilerHostFactory)
        {
            _logger = logger;
            _fileHandlerFactory = fileHandlerFactory;
            _compilerHostFactory = compilerHostFactory;
        }

        // Compile entire project.
        public async Task Compile(Uri documentUri, CancellationToken cancellationToken)
        {
            var fileHandler = _fileHandlerFactory.CreateFileHandler(documentUri);
            var compilerHost = _compilerHostFactory.CreateCompilerHost(fileHandler);
            var scopeUri = new Uri(documentUri.ToString());

            await Task.Run(() => compilerHost.Compile(scopeUri, cancellationToken));
        }
    }
}
