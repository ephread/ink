using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// Manage diagnosticians. Currently, a new diagnostician is created
    /// each time a request is received.
    ///
    /// <see cref="Diagnostician"/> for more information.
    /// </summary>
    public interface IDiagnosticManager
    {
        /// <summary>
        /// Compile and diagnose the document at the given Uri.
        /// </summary>
        /// <param name="documentUri">The Uri of the document to diagnose</param>
        /// <param name="cancellationToken">
        /// A cancellation token use to interrupt the search if a new
        /// request is received.
        /// </param>
        /// <returns></returns>
        Task CompileAndDiagnose(Uri documentUri, CancellationToken cancellationToken);
    }
}
