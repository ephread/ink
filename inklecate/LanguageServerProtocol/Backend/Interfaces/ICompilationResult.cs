using System;
using System.Collections.Generic;
using Ink.LanguageServerProtocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// Store compilation results.
    /// </summary>
    public interface ICompilationResult
    {
        Parsed.Story Story {get; set;}
        Stats Stats {get; set;}

        /// <summary>
        /// It's important to keep track of previous errors, since we need to
        /// clear the current diagnostics for each document involved in the
        /// compilation. However we don't want to wrongly erase diagnostics for
        /// unaffected documents (in the case of workspaces with no
        /// "main documents" defined).
        ///
        /// This is why errors are stored in the compilation results, as they
        /// are either scoped to the document triggering the compilation or the
        /// "main document" defined by the workspace.
        /// 
        /// <see cref="VirtualWorkspaceManager.GetCompilationResult(Uri)"/> for
        /// more information.
        /// </summary>
        Dictionary<Uri, List<CompilationError>> Errors {get; set;}
    }
}
