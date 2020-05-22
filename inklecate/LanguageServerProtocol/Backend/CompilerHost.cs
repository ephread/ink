using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Handle compilation and reporting, this object is intended to be
    /// used in a background thread.
    /// </summary>
    public class CompilerHost: ICompilerHost
    {
        private readonly ILogger<CompilerHost> _logger;
        private readonly ILanguageServerConnection _connection;
        private readonly IWorkspaceFileHandler _fileHandler;

        private readonly Dictionary<Uri, List<CompilationError>> _errors;

        public CompilerHost(
            ILogger<CompilerHost> logger,
            ILanguageServerConnection connection,
            IWorkspaceFileHandler fileHandler)
        {
            _logger = logger;
            _connection = connection;
            _fileHandler = fileHandler;

            _errors = new Dictionary<Uri, List<CompilationError>>();
        }

        // Compile entire project.
        public async Task Compile(Uri documentUri, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Retrieving main document URI…");
            var mainDocumentUri = await _fileHandler.GetMainDocument();

            _logger.LogDebug($"Retrieved. Uri is: '{mainDocumentUri}'");
            var inputString = _fileHandler.LoadDocumentContent(mainDocumentUri);

            ClearErrors();

            var compiler = new Compiler(inputString, new Compiler.Options {
                sourceFilename = mainDocumentUri.LocalPath,
                pluginNames = new List<string>(),
                countAllVisits = false,
                errorHandler = OnError,
                fileHandler = _fileHandler
            });

            using (_logger.TimeDebug("Compilation"))
            {
                compiler.Compile(cancellationToken);
            }

            PushDiagnosticsToClient(cancellationToken);
        }

        private void OnError(string message, ErrorType type)
        {
            // Parsing the message for now, but an another handler
            // should probably be created.
            _logger.LogDebug("Compiler reported an error.");
            var regex = new Regex(
                @"^(ERROR|WARNING|RUNTIME ERROR|RUNTIME WARNING|TODO): (?:'([^']+)')? line (\d+): (.+)",
                RegexOptions.Singleline);

            MatchCollection matches = regex.Matches(message);

            _logger.LogDebug($"Error parsed, found {matches.Count} match(es).");
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;

                var fileUri = _fileHandler.ResolveInkFileUri(groups[2].Value);
                if (!_errors.ContainsKey(fileUri))
                {
                    _errors[fileUri] = new List<CompilationError>();
                }

                _errors[fileUri].Add(new CompilationError() {
                    type = type,
                    file = fileUri,
                    lineNumber = Int32.Parse(groups[3].Value),
                    message = groups[4].Value
                });
            }
        }

        private void PushDiagnosticsToClient(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                _logger.LogDebug($"Cancellation was requested, no diagnostics to push.");
                return;
            }

            _logger.LogDebug($"Publishing {_errors.Count} file diagnostic(s) to client.");
            foreach (var KeyValue in _errors)
            {
                var diagnostics = KeyValue.Value.Select(error => {
                    return DiagnosticFromCompilationError(error);
                }).ToList();

                var diagnosticParams = new PublishDiagnosticsParams() {
                    Uri = KeyValue.Key,
                    Diagnostics = new Container<Diagnostic>(diagnostics)
                };

                if (diagnostics.Count > 0)
                {
                    _logger.LogDebug($"    -> Found {diagnostics.Count} diagnostics for '{diagnosticParams.Uri}'");
                }
                else
                {
                    _logger.LogDebug($"    -> Clearing diagnostics of '{diagnosticParams.Uri}'");
                }

                _connection.Document.PublishDiagnostics(diagnosticParams);
            }
        }

        private Diagnostic DiagnosticFromCompilationError(CompilationError error)
        {
            return new Diagnostic() {
                Range = RangeFromLineNumber(error.lineNumber),
                Severity = SeverityFromType(error.type),
                Source = "inklecate",
                Message = error.message
            };
        }

        private Range RangeFromLineNumber(int lineNumber)
        {
            return new Range(
                new Position(lineNumber - 1, 0),
                new Position(lineNumber, 0));
        }

        private DiagnosticSeverity SeverityFromType(ErrorType type)
        {
            switch (type)
            {
                case ErrorType.Author:
                    return DiagnosticSeverity.Information;
                case ErrorType.Warning:
                    return DiagnosticSeverity.Warning;
                case ErrorType.Error:
                    return DiagnosticSeverity.Error;
                default:
                    return DiagnosticSeverity.Error;
            }
        }

        private void ClearErrors()
        {
            foreach (var KeyValue in _errors)
            {
                KeyValue.Value.Clear();
            }
        }

        private struct CompilationError
        {
            public ErrorType type;
            public Uri file;
            public int lineNumber;
            public string message;
        }
    }
}