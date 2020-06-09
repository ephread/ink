using System;
using System.IO;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Extensions;
using Ink.LanguageServerProtocol.Models;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    public class SymbolStore: ISymbolStore
    {
        private Ink.Parsed.Object _rootObject;
        private Ink.Stats.Symbols _symbols;
        private IWorkspaceFileHandler _fileHandler;

        public Ink.Parsed.Object SymbolAt(Position position, Uri file)
        {
            return SymbolAt(position, file, _rootObject);
        }

        public void SetSyntaxTree(Ink.Parsed.Object rootObject)
        {
            _rootObject = rootObject;
        }

        public void SetSymbols(Ink.Stats.Symbols symbols)
        {
            _symbols = symbols;
        }

        // Should be O(log n)
        private Ink.Parsed.Object SymbolAt(Position position, Uri file, Ink.Parsed.Object @object)
        {
            if (@object.content.Count == 0) {
                // Current object is a leaf, does it contain the position?
                if (isObjectMatchingPositionAndFile(@object, position, file)) {
                    return @object;
                } else {
                    return null;
                }
            }

            // Not a leaf, checking children.
            foreach (var subObject in @object.content) {
                if (isObjectMatchingPositionAndFile(@object, position, file)) {
                    var result = SymbolAt(position, file, subObject);

                    if (result) {
                        return result;
                    }
                }
            }

            return null;
        }

        private bool isObjectMatchingPositionAndFile(
            Ink.Parsed.Object @object,
            Position position,
            Uri file)
        {
            return isPositionWithinObject(position, @object) &&
                   isObjectInFile(@object, file);
        }

        private bool isPositionWithinObject(Position position, Ink.Parsed.Object @object)
        {
            return position.Line >= @object.debugMetadata.startLineNumber &&
                   position.Line <= @object.debugMetadata.endLineNumber &&
                   position.Character >= @object.debugMetadata.startCharacterNumber &&
                   position.Character <= @object.debugMetadata.endCharacterNumber;
        }

        private bool isObjectInFile(Ink.Parsed.Object @object, Uri file)
        {
            var fileName = @object.debugMetadata.fileName;

            return _fileHandler.ResolveInkFilename(fileName) == file.LocalPath;
        }
    }
}