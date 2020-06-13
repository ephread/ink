using System;

namespace Ink.LanguageServerProtocol.Models {
    public struct CompilationError
    {
        public ErrorType type;
        public Uri file;
        public int lineNumber;
        public string message;
    }
}