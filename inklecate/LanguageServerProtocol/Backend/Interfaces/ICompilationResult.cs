using Ink;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface ICompilationResult
    {
        Parsed.Story Story {get; set;}
        Stats Stats {get; set;}
    }
}
