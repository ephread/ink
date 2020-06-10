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

        public SymbolStore(IWorkspaceFileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

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

        public static bool isPositionWithinDebugMetadata(Position position, Ink.Runtime.DebugMetadata metadata)
        {
            if (position.Line > (metadata.startLineNumber - 1) &&
                position.Line < (metadata.endLineNumber - 1))
            {
                return true;
            }

            if (position.Line == (metadata.startLineNumber - 1) &&
                position.Line == (metadata.endLineNumber - 1))
            {
                return position.Character >= (metadata.startCharacterNumber - 1) &&
                       position.Character <= (metadata.endCharacterNumber - 1);
            }

            if (position.Line >= (metadata.startLineNumber - 1) &&
                position.Line < (metadata.endLineNumber - 1))
            {
                return position.Character >= (metadata.startCharacterNumber - 1);
            }

            if (position.Line > (metadata.startLineNumber - 1) &&
                position.Line <= (metadata.endLineNumber - 1))
            {
                return position.Character <= (metadata.endCharacterNumber - 1);
            }

            return false;
        }

        // Should be O(log n)
        private Ink.Parsed.Object SymbolAt(Position position, Uri file, Ink.Parsed.Object @object)
        {
            if (@object.content == null || @object.content.Count == 0)
            {
                // Current object is a leaf, does it contain the position?
                if (isObjectMatchingPositionAndFile(@object, position, file))
                {
                    return @object;
                }
                else
                {
                    return null;
                }
            }

            // Not a leaf, checking children.
            foreach (var subObject in @object.content) {
                bool shouldDrillFurther = isWeaveOrHasNoMetadata(subObject) ||
                                          isObjectMatchingPositionAndFile(subObject, position, file);
                if (shouldDrillFurther)
                {
                    var result = SymbolAt(position, file, subObject);

                    if (result)
                    {
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
            if (@object?.debugMetadata == null)
            {
                return false;
            }

            return isPositionWithinObject(position, @object) &&
                   isObjectInFile(@object, file);
        }

        private bool isPositionWithinObject(Position position, Ink.Parsed.Object @object)
        {
            if (@object?.debugMetadata == null)
            {
                return false;
            }

            return isPositionWithinDebugMetadata(position, @object.debugMetadata);
        }

        private bool isObjectInFile(Ink.Parsed.Object @object, Uri file)
        {
            var fileName = @object.debugMetadata.fileName;

            return _fileHandler.ResolveInkFilename(fileName) == file.LocalPath;
        }

        private bool isWeaveOrHasNoMetadata(Ink.Parsed.Object @object)
        {
            return @object is Ink.Parsed.Weave || @object.debugMetadata == null;
        }
    }
}