using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Ink.LanguageServerProtocol.Backend.Interfaces;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    /// <summary>
    /// Store buffers of opened documents as well as compilation/diagnotics
    /// results and handle path resolution.
    /// </summary>
    public interface IVirtualWorkspaceManager
    {
        /// <summary>
        /// The root Uri of the workspace, as per configuration settings.
        /// </summary>
        Uri Uri { get; }

        TextDocumentItem GetTextDocument(Uri uri);
        void UpdateContentOfTextDocument(Uri uri, String text);
        void SetTextDocument(Uri uri, TextDocumentItem document);
        void RemoveTextDocument(Uri uri);

        ICompilationResult GetCompilationResult(Uri uri);
        void SetCompilationResult(Uri uri, ICompilationResult result);

        /// <summary>
        /// Convert a path (either local, partial or fully-qualified) into
        /// a fully-qualified Uri.
        /// <see cref="ResolvePath(string path, Uri mainDirectoryUri)" /> for
        /// more information.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>A fully qualified Uri</returns>
        Uri ResolvePath(string path);

        /// <summary>
        /// Convert a path (either local, partial or fully-qualified) into
        /// a fully-qualified Uri, using an optional mainDirectoryUri as
        /// a prefix.
        ///
        /// If the given path is already fully-qualified
        /// (i. e. starts with  something like <c>/</c> or <c>c:</c>) then
        /// this method converts it to a Uri without transformation.
        ///
        /// Otherwise, the path is prefixed with <c>mainDirectoryUri</c>.
        /// If <c>mainDirectoryUri</c> is <c>null</c>, <c>this.Uri</c> is used.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mainDirectoryUri"></param>
        /// <returns></returns>
        Uri ResolvePath(string path, Uri mainDirectoryUri);
    }
}
