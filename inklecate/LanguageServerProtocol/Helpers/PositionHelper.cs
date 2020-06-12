
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Helpers
{
    public class PositionHelper
    {
        public static bool isObjectAtPosition(Parsed.Object @object, Position position)
        {
            if (@object?.debugMetadata == null)
            {
                return false;
            }

            return isMetadataContainingPosition(@object.debugMetadata, position);
        }

        public static bool isIdentifierAtPosition(Parsed.Identifier identifier, Position position)
        {
            if (identifier?.debugMetadata == null)
            {
                return false;
            }

            return isMetadataContainingPosition(identifier.debugMetadata, position);
        }

        public static bool isMetadataContainingPosition(Runtime.DebugMetadata metadata, Position position)
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
    }
}