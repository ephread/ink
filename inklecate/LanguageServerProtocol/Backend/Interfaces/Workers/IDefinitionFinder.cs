using System;
using System.Threading;
using System.Threading.Tasks;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDefinitionFinder
    {
        LocationOrLocationLinks GetDefinition(Position position, CancellationToken cancellationToken);
        Hover GetHover(Position position, CancellationToken cancellationToken);
        Task retrieveMainDocument();
    }
}
