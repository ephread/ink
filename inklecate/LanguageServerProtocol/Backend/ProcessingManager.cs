using System;
using System.Collections.Generic;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;


namespace Ink.LanguageServerProtocol.Backend
{
    // Compile or run static analysis, then push diagnostics back to the client.
    public class ProcessingManager: IProcessingManager
    {
        private ILogger<ProcessingManager> _logger;
        private Ink.IFileHandler _fileHandler;
        private ILanguageServerConnection _connection;
        private IVirtualWorkspaceManager _workspace;

        private List<string> _errors = new List<string>();
        private List<string> _warnings = new List<string>();
        private List<string> _authorMessages = new List<string>();

        public ProcessingManager(
            ILogger<ProcessingManager> logger,
            ILanguageServerConnection connection,
            IVirtualWorkspaceManager workspace,
            Ink.IFileHandler fileHandler)
        {
            _logger = logger;
            _connection = connection;
            _workspace = workspace;
            _fileHandler = fileHandler;
        }

        // Compilation or analysis should be done in cancellable tasks.

        // Compile entire project.
        public void Compile()
        {
            var inputString = loadEntryPoint();
            var compiler = new Compiler(inputString, new Compiler.Options {
                sourceFilename = "",
                pluginNames = new List<string>(),
                countAllVisits = false,
                errorHandler = OnError,
                fileHandler = _fileHandler
            });

            compiler.Compile();

            ClearDiagnostics();
            PushDiagnostics();
        }

        // Parse file to get fast analysis. To be implemented.
        public void Analyse(Uri uri)
        {

        }

        private string loadEntryPoint() {
            return "";
        }

        private void OnError(string message, ErrorType type)
        {
            switch (type) {
            case ErrorType.Author:
                _authorMessages.Add(message);
                break;

            case ErrorType.Warning:
                _warnings.Add(message);
                break;

            case ErrorType.Error:
                _errors.Add(message);
                break;
            }
        }

        private void ClearDiagnostics()
        {
            _errors.Clear();
            _warnings.Clear();
            _authorMessages.Clear();
        }

        private void PushDiagnostics()
        {
            var diagnostics = BuildDiagnostics();
            //_server.Document.PublishDiagnostics();
        }

        private List<Diagnostic> BuildDiagnostics()
        {
            foreach (var error in _errors)
            {
                _logger.LogDebug("[BACKEND]" + error);
            }

            foreach (var warning in _warnings)
            {
                _logger.LogDebug("[BACKEND]" + warning);
            }

            foreach (var authorMessage in _authorMessages)
            {
                _logger.LogDebug("[BACKEND]" + authorMessage);
            }

            return new List<Diagnostic>();
        }
    }
}
