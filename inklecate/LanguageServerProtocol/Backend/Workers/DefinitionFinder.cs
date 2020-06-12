using System;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Extensions;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    public class DefinitionFinder: IDefinitionFinder
    {
        private readonly ILogger<DefinitionFinder> _logger;
        private readonly IVirtualWorkspaceManager _workspace;
        private readonly IWorkspaceFileHandler _fileHandler;

        private readonly SymbolResolver _symbolResolver;
        private readonly DefinitionResolver _definitionResolver;

        private readonly Uri _documentUri;
        private Uri _mainDocumentUri;

    /* ********************************************************************** */

        public DefinitionFinder(
            ILogger<DefinitionFinder> logger,
            IVirtualWorkspaceManager workspace,
            IWorkspaceFileHandler fileHandler,
            Uri documentUri)
        {
            _logger = logger;
            _workspace = workspace;
            _fileHandler = fileHandler;
            _documentUri = documentUri;

            _symbolResolver = new SymbolResolver(_fileHandler);
            _definitionResolver = new DefinitionResolver(_symbolResolver, _fileHandler);
        }

    /* ********************************************************************** */

        public LocationOrLocationLinks GetDefinition(Position position, CancellationToken cancellationToken)
        {
            var compilationResult = _workspace.GetCompilationResult(_mainDocumentUri);

            if (compilationResult == null)
            {
                return null;
            }

            _symbolResolver.Story = compilationResult.Story;

            LocationOrLocationLinks result;
            using (_logger.TimeDebug("Definition Resolution"))
            {
                result = _definitionResolver.DefinitionForSymbolAt(position, _documentUri, cancellationToken);
            }

            return result;
        }

        public async Task retrieveMainDocument()
        {
            _mainDocumentUri = await _fileHandler.GetMainDocument();
        }
    }
}
