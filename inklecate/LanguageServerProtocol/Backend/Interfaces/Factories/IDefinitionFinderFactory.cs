using System;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDefinitionFinderFactory
    {
        IDefinitionFinder CreateDefinitionFinder(Uri documentUri);
    }
}
