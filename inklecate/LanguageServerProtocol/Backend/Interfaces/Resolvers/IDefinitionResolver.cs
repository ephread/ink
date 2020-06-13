using System;
using System.Threading;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// Resolve Definitions
    /// </summary>
    public interface IDefinitionResolver
    {
        /// <summary>
        /// Resolve the definition for the token at position, in file.
        ///
        /// If the token is not a token with a definition,
        /// return an empty LocationOrLocationLinks.
        ///
        /// Note: if there is no compilation results stored in the workspace,
        /// return an empty LocationOrLocationLinks.
        /// </summary>
        /// <param name="position">The position to test</param>
        /// <param name="file">The file to which position refers to</param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns>The location of the definition</returns>
        LocationOrLocationLinks DefinitionForSymbolAt(
            Position position,
            Uri file,
            CancellationToken cancellationToken);
    }
}