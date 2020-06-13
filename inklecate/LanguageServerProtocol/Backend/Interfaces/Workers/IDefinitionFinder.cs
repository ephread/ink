using System;
using System.Threading;
using System.Threading.Tasks;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// 1 - Get definition at the given position
    /// 2 - Get hover message at the given position
    ///
    /// A DefinitionFinder is tied to a specific document.
    /// </summary>
    public interface IDefinitionFinder
    {
        /// <summary>
        /// Get the definition location for the token at the given position.
        ///
        /// If the token has no definition, or there are no compilation
        /// results in the workspace return an empty LocationOrLocationLinks.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns></returns>
        LocationOrLocationLinks GetDefinition(
            Position position,
            CancellationToken cancellationToken);

        /// <summary>
        /// Get the hover message for the token at the given position.
        ///
        /// If the token has no message, or there are no compilation
        /// results in the workspace return an empty Hover.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns></returns>
        Hover GetHover(Position position, CancellationToken cancellationToken);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task RetrieveMainDocument();
    }
}
