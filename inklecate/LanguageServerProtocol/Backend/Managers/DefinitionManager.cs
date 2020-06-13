using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    public class DefinitionManager: IDefinitionManager
    {
        private readonly ILogger<DefinitionManager> _logger;
        private readonly IDefinitionFinderFactory _definitionFinderFactory;

        private readonly Dictionary<Uri, IDefinitionFinder> _definitionFinders;

        public DefinitionManager(
            ILogger<DefinitionManager> logger,
            IDefinitionFinderFactory definitionFinderFactory)
        {
            _logger = logger;
            _definitionFinderFactory = definitionFinderFactory;

            _definitionFinders = new Dictionary<Uri, IDefinitionFinder>();
        }

        public async Task<LocationOrLocationLinks> GetDefinition(
            Position position,
            Uri documentUri,
            CancellationToken cancellationToken)
        {
            var definitionFinder = await GetDefinitionFinder(documentUri);

            return definitionFinder.GetDefinition(position, cancellationToken);
        }

        public async Task<Hover> GetHover(
            Position position,
            Uri documentUri,
            CancellationToken cancellationToken)
        {
            var definitionFinder = await GetDefinitionFinder(documentUri);

            return definitionFinder.GetHover(position, cancellationToken);
        }

        public void RemoveDefinitionFinder(Uri documentUri)
        {
            _logger.LogDebug($"Removing DefinitionFinder at: '{documentUri}'");
            _definitionFinders.Remove(documentUri);
        }

        public void RemoveAllDefinitionFinders()
        {
            _logger.LogDebug("Removing all DefinitionFinders.'");
            _definitionFinders.Clear();
        }

        private async Task<IDefinitionFinder> GetDefinitionFinder(Uri documentUri)
        {
            if (!_definitionFinders.TryGetValue(documentUri, out IDefinitionFinder definitionFinder))
            {
                _logger.LogDebug($"Creating new DefinitionFinder for: '{documentUri}'");
                definitionFinder = _definitionFinderFactory.CreateDefinitionFinder(documentUri);
                await definitionFinder.RetrieveMainDocument();

                _definitionFinders[documentUri] = definitionFinder;
            }

            return definitionFinder;
        }
    }
}
