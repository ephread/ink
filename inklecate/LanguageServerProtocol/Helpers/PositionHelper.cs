
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Helpers
{
    /// <summary>
    /// Provide a set of methods to check wether a given
    /// Parsed.Identifier / Parsed.Object contains a given position.
    /// </summary>
    public class PositionHelper
    {
        /// <summary>
        /// Check wether the given object contains the given position.
        /// This method return false if the object or its Debug Metadata
        /// is <c>null</c>.
        /// </summary>
        /// <param name="object">The object to test against</param>
        /// <param name="position">The position to test</param>
        /// <returns>
        /// <c>true</c> if the object contains the position,
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsObjectAtPosition(Parsed.Object @object, Position position)
        {
            if (@object?.debugMetadata == null)
            {
                return false;
            }

            return IsMetadataContainingPosition(@object.debugMetadata, position);
        }

        /// <summary>
        /// Check wether the given identifier contains the given position.
        /// This method return false if the identifier or its Debug Metadata
        /// is <c>null</c>.
        /// </summary>
        /// <param name="identifier">The identifier to test against</param>
        /// <param name="position">The position to test</param>
        /// <returns>
        /// <c>true</c> if the identifier contains the position,
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsIdentifierAtPosition(Parsed.Identifier identifier, Position position)
        {
            if (identifier?.debugMetadata == null)
            {
                return false;
            }

            return IsMetadataContainingPosition(identifier.debugMetadata, position);
        }

        /// <summary>
        /// Check wether the object/identifier tagged with given
        /// metadata contains the position.
        /// </summary>
        /// <param name="metadata">The metadata to test against</param>
        /// <param name="position">The position to test</param>
        /// <returns>
        /// <c>true</c> if the object/identifier tagged with metadata
        /// contains the position, <c>false</c> otherwise.
        /// </returns>
        public static bool IsMetadataContainingPosition(Runtime.DebugMetadata metadata, Position position)
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