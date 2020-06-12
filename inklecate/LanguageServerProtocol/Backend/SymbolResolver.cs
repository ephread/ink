using System;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Helpers;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    public class SymbolResolver: ISymbolResolver
    {
        public Ink.Parsed.Story Story { get; set; }
        private readonly IWorkspaceFileHandler _fileHandler;

        public SymbolResolver(IWorkspaceFileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

        /// <summary>
        /// Find the symbol matching the given position in the given file
        /// recursively.
        ///
        /// </summary>
        /// <param name="position">position to match against (cursor position)</param>
        /// <param name="file">the file in which look for the symbol</param>
        /// <returns>The symbol if found or null otherwise.</returns>
        public object SymbolAt(Position position, Uri file)
        {
            if (Story == null)
            {
                return null;
            }

            return SymbolAt(position, file, Story);
        }

        /// <summary>
        /// Find the symbol matching the given position in the given file
        /// recursively.
        ///
        /// </summary>
        /// <param name="position">position to match against (cursor position)</param>
        /// <param name="file">the file in which look for the symbol</param>
        /// <param name="object">the current node</param>
        /// <returns>The symbol if found or null otherwise.</returns>
        private object SymbolAt(Position position, Uri file, Ink.Parsed.Object @object)
        {
            // Handling choices in a specific branch, since they can be labeled
            // or conditional.
            if (@object is Parsed.Choice choice)
            {
                // The cursor is above the choice's label, we can't got further
                // and return the corresponding identifier.
                if (isIdentifierMatchingPositionAndFile(choice.identifier, position, file))
                {
                    return choice.identifier;
                }

                // The cursor is above the choice's condition, we're drilling
                // into the condition.
                if (isObjectMatchingPositionAndFile(choice.condition, position, file))
                {
                    // Not that we don't check wether `SymbolAt` returns null.
                    // because this is a terminal path. If no matching
                    // symbol is found in the condition, there's no point
                    // in backtracking to test against other types.
                    return SymbolAt(position, file, choice.condition);
                }
            }

            // Handling Gathers with a specific branch, since they can be
            // labeled as well.
            if (@object is Parsed.Gather gather)
            {
                // The cursor is above the gather's label, we can't got further
                // and return the corresponding identifier.
                if (isIdentifierMatchingPositionAndFile(gather.identifier, position, file))
                {
                    return gather.identifier;
                }
            }

            // Handling External Functions separately, as they're also
            // implementing INamedContent.
            if (@object is Parsed.ExternalDeclaration declaration)
            {
                if (isIdentifierMatchingPositionAndFile(declaration.identifier, position, file))
                {
                    return declaration.identifier;
                }
            }

            // Handling Function Calls separately, since we might need to drill
            // into their arguments. Function calls can both be node and leafs,
            // so their content children are handled by the generic branch.
            //
            // To sumarise, FunctionCall can return:
            //    1. An object from its arguments (this branch)
            //    2. An object from its children, although in pratice,
            //       his children are unlikley to be matched (generic branch)
            //    3. Itself (token validity branch)
            if (@object is Parsed.FunctionCall functionCall)
            {
                if (functionCall.arguments != null)
                {
                    foreach (var argument in functionCall.arguments)
                    {
                        if (isObjectMatchingPositionAndFile(argument, position, file))
                        {
                            var result = SymbolAt(position, file, argument);

                            if (result != null)
                            {
                                return result;
                            }
                        }
                    }
                }
            }

            // Generic handling for leaves.
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

            // Generic handling for nodes. It's not a leaf, so we're checking
            // the content children.
            foreach (var subObject in @object.content) {
                // Weaves or objects without metadata are ignored
                // and passed through.
                bool shouldDrillFurther = isWeaveOrHasNoMetadata(subObject) ||
                                          isObjectMatchingPositionAndFile(subObject, position, file);
                if (shouldDrillFurther)
                {
                    var result = SymbolAt(position, file, subObject);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            // Certain objects might be considered "terminal symbols" even
            // even though they have content. See isValidToken.
            //
            // Since these symbols have their children stored in content,
            // the general logic checks them first.
            if (isValidToken(@object))
            {
                return @object;
            }
            // FlowBase also have specific handling since:
            //     1. it's undesirable to return the entire object when
            //        the position is matching the declaration identifier
            //     2. FlowBase can define parameters.
            else if (@object is Parsed.FlowBase flowBase)
            {
                if (flowBase.arguments != null)
                {
                    foreach (var argument in flowBase.arguments)
                    {
                        if (isIdentifierMatchingPositionAndFile(argument.identifier, position, file))
                        {
                            return argument.identifier;
                        }
                    }
                }

                if (isIdentifierMatchingPositionAndFile(flowBase.identifier, position, file))
                {
                    return flowBase.identifier;
                }

                return null;
            }
            else
            {
                return null;
            }
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

            return PositionHelper.isObjectAtPosition(@object, position) &&
                   isObjectInFile(@object, file);
        }

        private bool isIdentifierMatchingPositionAndFile(
            Parsed.Identifier identifier,
            Position position,
            Uri file)
        {
            if (identifier?.debugMetadata == null)
            {
                return false;
            }

            return PositionHelper.isIdentifierAtPosition(identifier, position) &&
                   isFileInMetadata(identifier.debugMetadata, file);
        }

        private bool isObjectInFile(Ink.Parsed.Object @object, Uri file)
        {
            if (@object == null)
            {
                return false;
            }

            return isFileInMetadata(@object.debugMetadata, file);
        }

        private bool isFileInMetadata(Runtime.DebugMetadata metadata, Uri file)
        {
            if (metadata == null)
            {
                return false;
            }


            return _fileHandler.ResolveInkFilename(metadata.fileName) == file.LocalPath;
        }

        private bool isWeaveOrHasNoMetadata(Ink.Parsed.Object @object)
        {
            return @object is Ink.Parsed.Weave || @object.debugMetadata == null;
        }

        /// <summary>
        /// Sometimes a node can be the object we need if:
        ///     1. none of its children narrow down the location;
        ///     2. the object is considered a valid end token.
        /// </summary>
        /// <param name="object">the object to test</param>
        /// <returns>
        ///     true if the object has a type matching a valid token.
        /// </returns>
        private bool isValidToken(Parsed.Object @object)
        {
            return @object is Parsed.VariableAssignment ||
                   @object is Parsed.ConstantDeclaration ||
                   @object is Parsed.FunctionCall ||
                   @object is Parsed.IncDecExpression ||
                   @object is Parsed.UnaryExpression;
        }
    }
}