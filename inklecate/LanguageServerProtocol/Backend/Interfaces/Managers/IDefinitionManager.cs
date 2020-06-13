using System;
using System.Threading;
using System.Threading.Tasks;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// Manage a set of DefinitionFinders, per document.
    ///
    /// When a request is processed this manager will look for a previously
    /// created DefinitionFinder or create a new one if it doesn't exist.
    ///
    /// Caching DefinitionFinders is an important optimisation as they
    /// fetch the configuration after their creation.
    ///
    /// <see cref="DefinitionResolver"/> for more information.
    /// </summary>
    public interface IDefinitionManager
    {
        /// <summary>
        /// Get the definition for the given position in the given file.
        /// </summary>
        /// <param name="position">The position to use</param>
        /// <param name="File">The file to which the position refers to</param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns></returns>
        Task<LocationOrLocationLinks> GetDefinition(
            Position position,
            Uri File,
            CancellationToken cancellationToken);

        /// <summary>
        /// Get the hover message for the given position in the given file.
        /// </summary>
        /// <param name="position">The position to use</param>
        /// <param name="File">The file to which the position refers to</param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns></returns>
        Task<Hover> GetHover(Position position, Uri File, CancellationToken cancellationToken);

        /// <summary>
        /// Remove the DefinitionFinder cached for the given Uri.
        /// This method is expected to be called when the related
        /// document is closed.
        /// </summary>
        /// <param name="documentUri">
        /// The document Uri matching the DefinitionFinder
        /// </param>
        void RemoveDefinitionFinder(Uri documentUri);

        /// <summary>
        /// Remove all the cached DefinitionFinder.
        /// </summary>
        void RemoveAllDefinitionFinders();
    }
}
