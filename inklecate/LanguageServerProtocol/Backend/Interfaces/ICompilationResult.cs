using Ink;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    /// <summary>
    /// Store compilation results.
    /// </summary>
    public interface ICompilationResult
    {
        Parsed.Story Story {get; set;}
        Stats Stats {get; set;}
    }
}
