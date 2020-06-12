using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    // Compile or run static analysis, then push diagnostics back to the client.
    public class DefinitionManager: IDefinitionManager
    {
        private readonly ILogger<DefinitionManager> _logger;
        private readonly IDefinitionFinderFactory _definitionFinderFactory;

        private Dictionary<Uri, IDefinitionFinder> _definitionFinders;

        public DefinitionManager(
            ILogger<DefinitionManager> logger,
            IDefinitionFinderFactory definitionFinderFactory)
        {
            _logger = logger;

            _definitionFinderFactory = definitionFinderFactory;

            _definitionFinders = new Dictionary<Uri, IDefinitionFinder>();
        }

        public async Task<LocationOrLocationLinks> GetDefinition(Position position, Uri documentUri, CancellationToken cancellationToken)
        {
            IDefinitionFinder definitionFinder;
            if (!_definitionFinders.TryGetValue(documentUri, out definitionFinder))
            {
                definitionFinder = _definitionFinderFactory.CreateDefinitionFinder(documentUri);
                await definitionFinder.retrieveMainDocument();

                _definitionFinders[documentUri] = definitionFinder;
            }

            return definitionFinder.GetDefinition(position, cancellationToken);
        }

        public void RemoveDefinitionFinder(Uri documentUri)
        {
            _definitionFinders.Remove(documentUri);
        }

        public void RemoveAllDefinitionFinders()
        {
            _definitionFinders.Clear();
        }
    }
}
