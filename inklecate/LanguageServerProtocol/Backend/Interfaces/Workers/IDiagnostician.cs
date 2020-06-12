using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDiagnostician
    {
        Task<List<Uri>> CompileAndDiagnose(List<Uri> previousFilesWithErrors);
    }
}
