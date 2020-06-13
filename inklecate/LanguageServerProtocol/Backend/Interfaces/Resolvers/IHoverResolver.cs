using System;
using System.Threading;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// Resolve hover messages.
    /// </summary>
    public interface IHoverResolver
    {
        /// <summary>
        /// Resolve the hover message for the token at position, in file.
        ///
        /// If the token is not a token with a hover message,
        /// return an empty Hover.
        ///
        /// Note: if there is no compilation results stored in the workspace,
        /// return an empty Hover.
        /// </summary>
        /// <param name="position">The position to test</param>
        /// <param name="file">The file to which position refers to</param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns>The hover message</returns>
        Hover HoverForSymbolAt(Position position, Uri file, CancellationToken cancellationToken);
    }
}