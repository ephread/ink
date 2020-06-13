using System;
using System.Threading.Tasks;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    /// <summary>
    /// 1 - Resolve the location of file names and then ask the virtual
    /// workspace to either read files from the disk or return
    /// in-memory buffers.
    ///
    /// 2 - Request the main document Uri from the client
    /// (configuration setting). The main document Uri is the entry point
    /// of the story.
    /// </summary>
    public interface IWorkspaceFileHandler: IFileHandler
    {
        /// <summary>
        /// Load the most recent version of the document at the given Uri,
        /// regardless of whether the file is closed/opned/has changes.
        /// </summary>
        /// <param name="uri">The location of the document</param>
        /// <returns>The content of the document</returns>
        string LoadDocumentContent(Uri uri);

        /// <summary>
        /// Asynchronously define what is the "main document". A main document
        /// is a critical part of the file resolution system, as includes are
        /// resolved relatively to this file.
        ///
        /// There are two possible options:
        ///
        /// 1 - The client has defined an entry point in the
        ///     configuration settings; this file will be the "main document".
        /// 2 - Otherwise, the internal document Uri of the file handler
        ///     will be used as the "main document"
        ///
        /// Note: the main document is also used to store compilation results.
        /// </summary>
        /// <returns></returns>
        Task<Uri> GetMainDocument();

        /// <summary>
        /// Resolve a Uri from an include name. "Include names" are sent
        /// by the parser when requesting the content of an included file.
        ///
        /// They are most of time relative to the entry point (either the
        /// current document or the main document),
        /// </summary>
        /// <param name="includeName">The relative path to the document</param>
        /// <returns>A fully qualified Uri</returns>
        Uri ResolveInkFileUri(string includeName);
    }
}
