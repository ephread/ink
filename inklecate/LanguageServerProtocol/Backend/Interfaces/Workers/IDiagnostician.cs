using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// 1 - Compile
    /// 2 - Diagnose errors push them to the client.
    /// 3 - Store compilation results into the workspace for further use.
    /// </summary>
    public interface IDiagnostician
    {
        /// <summary>
        /// Recompile the project and push any error encountered
        /// to the client.
        ///
        /// Note that pushing errors also means clearing any
        /// previous errors raised.
        /// </summary>
        /// <param name="previousFilesWithErrors">
        /// The previous files containing errors (used to clear old errors).
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns>A list of files containing errors</returns>
        Task<List<Uri>> CompileAndDiagnose(
            List<Uri> previousFilesWithErrors,
            CancellationToken cancellationToken);
    }
}
