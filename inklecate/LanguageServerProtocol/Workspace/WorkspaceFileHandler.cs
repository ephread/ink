using System;
using System.IO;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Models;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Ink.LanguageServerProtocol
{
    // Will resolve file names and then ask the virtual workspace to
    // either read files from the disk or return in-memory buffers.
    public class WorkspaceFileHandler: IWorkspaceFileHandler
    {
        private readonly ILogger<WorkspaceFileHandler> _logger;
        private readonly ILanguageServerEnvironment _environment;
        private readonly ILanguageServerConnection _connection;
        private readonly IVirtualWorkspaceManager _workspace;

        private readonly Uri _scopeUri;

        private InkConfiguration inkConfiguration = InkConfiguration.Default;

        private Uri RootInkFile
        {
            get {
                // If the client provided a main entry point, that file is
                // the root file.
                if (inkConfiguration.IsMainStoryDefined)
                {
                    return new Uri(_environment.RootUri, inkConfiguration.mainStoryPath);
                }
                else // Otherwise, we treat the current file as the root file.
                {
                    return _scopeUri;
                }
            }
        }

/* ************************************************************************** */

        public WorkspaceFileHandler(
            ILogger<WorkspaceFileHandler> logger,
            ILanguageServerEnvironment environment,
            ILanguageServerConnection connection,
            IVirtualWorkspaceManager workspace,
            Uri scopeUri)
        {
            _logger = logger;
            _environment = environment;
            _connection = connection;
            _workspace = workspace;

            _scopeUri = scopeUri;
        }

/* ************************************************************************** */

        public string LoadDocumentContent(Uri uri)
        {
            if (!_environment.RootUri.IsBaseOf(uri)) {
                throw new Exception("The specified document is outside of the current workspace.");
            } else {
                TextDocumentItem documentItem = _workspace.GetTextDocument(uri);
                if (documentItem != null) {
                    _logger.LogInformation($"(FILE HANDLER) Loading '{uri}' from memory.");
                    return documentItem.Text;
                } else {
                    _logger.LogInformation($"(FILE HANDLER) Loading '{uri}' from disk.");
                    var text = File.ReadAllText(uri.LocalPath);
                    _logger.LogInformation($"(FILE HANDLER) Loaded.");
                    return text;
                }
            }
        }

        // Fetch the entry point defined by the client for the scope URI.
        public async Task<Uri> GetMainDocument()
        {
            var configurationParams = new ConfigurationParams() {
                Items = new Container<ConfigurationItem>(new ConfigurationItem() {
                    ScopeUri = _scopeUri,
                    Section = "ink"
                })
            };

            var configurationContainer = await _connection.Workspace.WorkspaceConfiguration(configurationParams);

            // Only picking up the first configuration item for now.
            var enumerator = configurationContainer.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var jToken = enumerator.Current;
                inkConfiguration = new InkConfiguration(inkConfiguration, jToken);
            }

            if (inkConfiguration.IsMainStoryDefined) {
                _logger.LogInformation("(FILE HANDLER) `mainStoryPath` is not provided, using current file as main file.");
            } else {
                _logger.LogInformation($"(FILE HANDLER) `mainStoryPath` is provided, using '{inkConfiguration.mainStoryPath}' as main file.");
            }

            return RootInkFile;
        }

        public string ResolveInkFilename (string includeName) {
            var resolvedPath = _workspace.GetUriFromRelativePath(RootInkFile, includeName).LocalPath;

            _logger.LogDebug($"(FILE HANDLER) Resolving '{includeName}' into '{resolvedPath}'");

            return resolvedPath;
        }

        public string LoadInkFileContents (string fullFilename) {
            var fileUri = _workspace.GetUriFromAbsolutePath(fullFilename);

            _logger.LogDebug($"(FILE HANDLER) Loading content of '{fullFilename}' using '{fileUri}'");

            return LoadDocumentContent(fileUri);
        }
    }
}
