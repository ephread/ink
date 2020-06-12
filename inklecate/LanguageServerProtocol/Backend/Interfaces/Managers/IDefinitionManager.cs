using System;
using System.Threading;
using System.Threading.Tasks;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDefinitionManager
    {
        Task<LocationOrLocationLinks> GetDefinition(Position position, Uri File, CancellationToken cancellationToken);
        void RemoveDefinitionFinder(Uri documentUri);
        void RemoveAllDefinitionFinders();
    }
}
