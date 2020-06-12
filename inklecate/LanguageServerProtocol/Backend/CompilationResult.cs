using Ink.LanguageServerProtocol.Backend.Interfaces;

namespace Ink.LanguageServerProtocol.Backend
{
    public class CompilationResult: ICompilationResult
    {
        public Parsed.Story Story {get; set;}
        public Stats Stats {get; set;}
    }
}