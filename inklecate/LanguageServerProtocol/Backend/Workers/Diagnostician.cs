﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Extensions;
using Ink.LanguageServerProtocol.Models;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using Ink.LanguageServerProtocol.Helpers;

namespace Ink.LanguageServerProtocol.Backend
{
    // Compile or run static analysis, then push diagnostics back to the client.
    public class Diagnostician: IDiagnostician
    {
        private readonly ILogger<Diagnostician> _logger;
        private readonly ILanguageServerConnection _connection;
        private readonly IVirtualWorkspaceManager _workspace;
        private readonly IWorkspaceFileHandler _fileHandler;

        private Dictionary<Uri, List<CompilationError>> _errors;

/* ************************************************************************** */

        public Diagnostician(
            ILogger<Diagnostician> logger,
            ILanguageServerConnection connection,
            IVirtualWorkspaceManager workspace,
            IWorkspaceFileHandler fileHandler)
        {
            _logger = logger;
            _connection = connection;
            _workspace = workspace;
            _fileHandler = fileHandler;
        }

/* ************************************************************************** */

        // Compile entire project.
        public async Task CompileAndDiagnose(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Retrieving main document URI…");

            var mainDocumentUri = await _fileHandler.ResolveMainDocument();

            _logger.LogDebug($"Retrieved. Uri is: '{mainDocumentUri}'");

            PrepareErrors(mainDocumentUri);

            var inputString = _fileHandler.LoadDocumentContent(mainDocumentUri);

            var compiler = new Compiler(inputString, new Compiler.Options {
                sourceFilename = mainDocumentUri.LocalPath,
                pluginNames = new List<string>(),
                countAllVisits = false,
                errorHandler = OnError,
                fileHandler = _fileHandler
            });

            using (_logger.TimeDebug("Compilation"))
            {
                using (_logger.TimeDebug("Parsing"))
                {
                    compiler.Parse();
                }

                if (cancellationToken.IsCancellationRequested) return;

                Stats stats;
                using (_logger.TimeDebug("Statistics Generation"))
                {
                    Stats.Symbols symbols = Stats.Symbols.Generate(compiler.parsedStory);
                    stats = Stats.Generate(symbols);

                    PublishStatisticsToClient(_workspace.Uri, mainDocumentUri, stats);
                }

                if (cancellationToken.IsCancellationRequested) return;

                using (_logger.TimeDebug("Code Generation"))
                {
                    compiler.Generate();
                }

                _workspace.SetCompilationResult(mainDocumentUri, new CompilationResult() {
                    Story = compiler.parsedStory,
                    Stats = stats,
                    Errors = CopyErrors(_errors)
                });
            }

            PublishDiagnosticsToClient();
        }

/* ************************************************************************** */

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

        private void PublishDiagnosticsToClient()
        {
            _logger.LogDebug($"Publishing {_errors.Count} file diagnostic(s) to client.");
            foreach (var KeyValue in _errors)
            {
                var diagnostics = KeyValue.Value.Select(error => {
                    return DiagnosticFromCompilationError(error);
                }).ToList();

                var diagnosticParams = new PublishDiagnosticsParams() {
                    Uri = UriHelper.ToClientUri(KeyValue.Key),
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

        private void PublishStatisticsToClient(
            Uri workspaceUri,
            Uri mainDocumentUri,
            Ink.Stats stats)
        {
            _logger.LogDebug("Publishing statistics to client");
            _connection.Client.SendNotification("story/statistics", new StatisticsParams() {
                WorkspaceUri = workspaceUri,
                MainDocumentUri = mainDocumentUri,
                Statistics = stats,
            });
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
            return type switch
            {
                ErrorType.Author => DiagnosticSeverity.Information,
                ErrorType.Warning => DiagnosticSeverity.Warning,
                ErrorType.Error => DiagnosticSeverity.Error,
                _ => DiagnosticSeverity.Error,
            };
        }

        /// <summary>
        /// Retrieve errors from the previous compilation results stored in
        /// the workspace.
        /// </summary>
        /// <param name="mainDocumentUri"></param>
        private void PrepareErrors(Uri mainDocumentUri)
        {
            var compilationResult = _workspace.GetCompilationResult(mainDocumentUri);
            if (compilationResult != null)
            {
                _errors = CopyErrors(compilationResult.Errors);

                foreach (var keyValue in _errors)
                {
                    keyValue.Value.Clear();
                }
            }
            else
            {
                _errors = new Dictionary<Uri, List<CompilationError>>();
            }
        }

        private Dictionary<Uri, List<CompilationError>> CopyErrors(Dictionary<Uri, List<CompilationError>> errors)
        {
            return errors.ToDictionary(p => p.Key, p => p.Value.ToList());
        }
    }
}
