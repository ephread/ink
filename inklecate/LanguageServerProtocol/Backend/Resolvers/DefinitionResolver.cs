using System;
using System.Threading;
using System.Linq;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Helpers;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    public class DefinitionResolver: IDefinitionResolver
    {
        private readonly ISymbolResolver _symbolResolver;
        private readonly IWorkspaceFileHandler _fileHandler;

        public DefinitionResolver(
            ISymbolResolver symbolResolver,
            IWorkspaceFileHandler fileHandler)
        {
            _symbolResolver = symbolResolver;
            _fileHandler = fileHandler;
        }


        /// <summary>
        /// Find the definition of the symbol at <c>position</c>, in <c>file</c>.
        /// </summary>
        /// <param name="position">position to match against (cursor position)</param>
        /// <param name="file">the file in which look for the symbol</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public LocationOrLocationLinks DefinitionForSymbolAt(Position position, Uri file, CancellationToken cancellationToken)
        {
            var parsedObject = _symbolResolver.SymbolAt(position, file, cancellationToken);

            if (cancellationToken.IsCancellationRequested) return null;

            if (parsedObject == null)
            {
                return null;
            }

            // If position matches the identifier of a INamedContent-conforming
            // object, SymbolAt returns that identifier.
            // It happens for Knots, Stitches, Choices, Gathers
            // and External Functions.
            //
            // The definition points back to itself, so we're
            // just returning the identifier.
            if (parsedObject is Parsed.Identifier declarationIdentifier)
            {
                return LocationFromMetadata(declarationIdentifier.debugMetadata);
            }

            if (cancellationToken.IsCancellationRequested) return null;

            // With diverts, the definition is resolved through Divert.Path.
            if (parsedObject is Parsed.Divert divert && divert.targetContent?.debugMetadata?.fileName != null)
            {
                // We treat each component of the Path separately
                // (knot.stitch.label), so that we can point to the
                // appropriate definition.
                var components = divert.target.identifiableComponents;
                for (int i = 0; i < components.Count; i++)
                {
                    var pathIdentifier = components[i];
                    if (PositionHelper.IsIdentifierAtPosition(pathIdentifier, position))
                    {
                        // If the component is the tail, we just
                        // use targetContent since the resolution was
                        // already performed.
                        if (i == components.Count)
                        {
                            return LocationFromMetadata(divert.targetContent.debugMetadata);
                        }
                        // Otherwise, we recreate a path with the current
                        // component as the tail and perform resolution again.
                        else
                        {
                            var partialComponents = components.Take(i + 1).ToList();
                            var newTarget = new Ink.Parsed.Path(partialComponents);
                            var targetContent = newTarget.ResolveFromContext(divert);

                            if (targetContent?.debugMetadata?.fileName != null)
                            {
                                // Sometimes the divert may be nested inside
                                // what it points to. Since the range of the
                                // target content encompasses the current
                                // location, returning the identifier makes sure
                                // that the cursor warps to the start of the FlowBase.
                                if (targetContent is Parsed.FlowBase flowBase && flowBase.identifier != null)
                                {
                                    return LocationFromMetadata(flowBase.identifier.debugMetadata);
                                }
                                else
                                {
                                    return LocationFromMetadata(targetContent.debugMetadata);
                                }
                            }
                        }
                    }
                }

                return null;
            }

            if (cancellationToken.IsCancellationRequested) return null;

            // With function calls, the definition is resolved through
            // FunctionCall.proxyDivert. Not that this branch only handles
            // function names; function parameters are handled by the
            // VariableReference branch.
            if (parsedObject is Parsed.FunctionCall functionCall && functionCall.proxyDivert?.target != null)
            {
                var targetContent = functionCall.proxyDivert.target.ResolveFromContext(functionCall.proxyDivert);
                if (targetContent)
                {
                    return LocationFromMetadata(targetContent.debugMetadata);
                }
                // Target is null, maybe it's an external function?
                else
                {
                    var externalFunction = _symbolResolver.Story.externals.TryGetValue(
                        functionCall.name,
                        out Parsed.ExternalDeclaration externalDeclaration
                    );

                    if (externalFunction && externalDeclaration?.identifier != null)
                    {
                        return LocationFromMetadata(externalDeclaration.identifier.debugMetadata);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested) return null;

            // With lists, we check every single list item and resolve them
            // to find the definition.
            // TODO: Expose the resolution logic in Parsed.List?
            if (parsedObject is Parsed.List list)
            {
                if (list.itemIdentifierList != null)
                {
                    foreach (var itemIdentifier in list.itemIdentifierList)
                    {
                        if (PositionHelper.IsIdentifierAtPosition(itemIdentifier, position))
                        {
                            var nameParts = itemIdentifier?.name.Split('.');

                            string listName = null;
                            string listItemName;
                            if (nameParts.Length > 1)
                            {
                                listName = nameParts[0];
                                listItemName = nameParts[1];
                            }
                            else
                            {
                                listItemName = nameParts[0];
                            }

                            var listItem = _symbolResolver.Story.ResolveListItem(listName, listItemName, list);
                            return LocationFromMetadata(listItem.debugMetadata);
                        }
                    }
                }
            }

            if (parsedObject is Parsed.ListElementDefinition listElementDefinition)
            {
                return LocationFromMetadata(listElementDefinition.identifier.debugMetadata);
            }

            if (cancellationToken.IsCancellationRequested) return null;

            if (parsedObject is Parsed.VariableReference variableReference)
            {
                // If it's a list item, perform list item resolution.
                // TODO: Expose the resolution logic in Parsed.VariableReference?
                if (variableReference.isListItemReference)
                {
                    var listPath = variableReference.path;

                    if (listPath.Count == 1 || listPath.Count == 2)
                    {
                        string listItemName = null;
                        string listName = null;

                        if (listPath.Count == 1)
                        {
                            listItemName = listPath[0];
                        }
                        else
                        {
                            listName = listPath[0];
                            listItemName = listPath[1];
                        }

                        var listItem = _symbolResolver.Story.ResolveListItem(listName, listItemName, variableReference);
                        return LocationFromMetadata(listItem.debugMetadata);
                    }
                    else
                    {
                        return null;
                    }
                }

                // If it's a constant reference, look for the constant
                // in Story.constants
                if (variableReference.isConstantReference)
                {
                    var constantExists = _symbolResolver.Story.constants.TryGetValue(
                        variableReference.name,
                        out Parsed.Expression constantExpression
                    );

                    if (constantExists)
                    {
                        if (constantExpression.parent is Parsed.ConstantDeclaration declaration)
                        {
                            return LocationFromMetadata(declaration.constantIdentifier.debugMetadata);
                        }
                    }
                }

                // If it's neither a list item nor a constant, check if this
                // variable exists in the global declarations.
                // Since ink doesn't allow variable-shadowing, neither temps
                // nor parameters can have the same names as a global variable.
                var variableExists = _symbolResolver.Story.variableDeclarations.TryGetValue(
                    variableReference.name,
                    out Parsed.VariableAssignment globalDeclaration
                );

                if (variableExists)
                {
                    return LocationFromMetadata(globalDeclaration.variableIdentifier.debugMetadata);
                }

                // Next, let go up the hierarchy to find either a
                // temp declaration or an argument of a function/knot.
                // Reassignments are ignored.
                var result = FindVariableDeclarationInParents(variableReference.parent, variableReference.name);
                if (result is Parsed.VariableAssignment variableDeclaration)
                {
                    return LocationFromMetadata(variableDeclaration.variableIdentifier.debugMetadata);
                }

                if (result is Parsed.FlowBase.Argument argument)
                {
                    return LocationFromMetadata(argument.identifier.debugMetadata);
                }

                // Finally, it might be a read count.
                var path = new Parsed.Path(variableReference.pathIdentifiers);
                var target = path.ResolveFromContext(variableReference);
                if (target)
                {
                    return LocationFromMetadata(target.debugMetadata);
                }

                return null;
            }

            if (cancellationToken.IsCancellationRequested) return null;

            if (parsedObject is Parsed.VariableAssignment variableAssignment)
            {
                // Assignements can be the definition point.
                if (variableAssignment.isDeclaration)
                {
                    return LocationFromMetadata(variableAssignment.variableIdentifier.debugMetadata);
                }

                // If it's not a declaration, we'll look in its ancestors to
                // find the delcaration (either a temp or a parameter).
                var result = FindVariableDeclarationInParents(variableAssignment.parent, variableAssignment.variableName);
                if (result is Parsed.VariableAssignment variableDeclaration)
                {
                    return LocationFromMetadata(variableDeclaration.variableIdentifier.debugMetadata);
                }

                if (result is Parsed.FlowBase.Argument argument)
                {
                    return LocationFromMetadata(argument.identifier.debugMetadata);
                }

                return null;
            }

            if (parsedObject is Parsed.ConstantDeclaration constantDeclaration)
            {
                return LocationFromMetadata(constantDeclaration.constantIdentifier.debugMetadata);
            }

            if (cancellationToken.IsCancellationRequested) return null;

            // Unary Expressions

            if (parsedObject is Parsed.IncDecExpression incDecExpression)
            {
                var variableName = incDecExpression.varIdentifier.name;

                // TODO: Share this code with VariableAssignment?
                // Check if it's a global variable.
                var variableExists = _symbolResolver.Story.variableDeclarations.TryGetValue(
                    variableName,
                    out Parsed.VariableAssignment globalDeclaration
                );

                if (variableExists)
                {
                    return LocationFromMetadata(globalDeclaration.variableIdentifier.debugMetadata);
                }

                // Next, similar to VariableAssignent let's go up the hierarchy
                // to find either a temp declaration or an argument
                // of a function/knot. Reassignments are still ignored.
                var result = FindVariableDeclarationInParents(incDecExpression.parent, variableName);
                if (result is Parsed.VariableAssignment variableDeclaration)
                {
                    return LocationFromMetadata(variableDeclaration.variableIdentifier.debugMetadata);
                }

                if (result is Parsed.FlowBase.Argument argument)
                {
                    return LocationFromMetadata(argument.identifier.debugMetadata);
                }
            }

            return null;
        }

        // TODO: Use ancestors?
        // TODO: Don't go higher up than the first FlowDecl, because of scoping?
        private object FindVariableDeclarationInParents(Parsed.Object @object, string variableName)
        {
            var variableAssignment = @object as Parsed.VariableAssignment;
            if (variableAssignment && variableAssignment.isDeclaration)
            {
                return variableAssignment;
            }

            var flowBase = @object as Parsed.FlowBase;
            if (flowBase)
            {
                Parsed.VariableAssignment localDeclaration;
                if (flowBase.variableDeclarations.TryGetValue(variableName, out localDeclaration))
                {
                    return localDeclaration;
                }

                if (flowBase.arguments?.Count > 0)
                {
                    foreach(var argument in flowBase.arguments)
                    {
                        if (argument.identifier.name == variableName)
                        {
                            return argument;
                        }
                    }
                }
            }

            if (@object.parent)
            {
                return FindVariableDeclarationInParents(@object.parent, variableName);
            }

            return null;
        }

        private LocationOrLocationLinks LocationFromMetadata(Ink.Runtime.DebugMetadata metadata)
        {
            if (metadata?.fileName == null)
            {
                return null;
            }

            var targetFile = _fileHandler.ResolveInkFileUri(metadata.fileName);

            return new LocationOrLocationLinks(new Location()
            {
                Uri = targetFile,
                Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range()
                {
                    Start = new Position()
                    {
                        Line = metadata.startLineNumber - 1,
                        Character = metadata.startCharacterNumber - 1
                    },
                    End = new Position()
                    {
                        Line = metadata.endLineNumber - 1,
                        Character = metadata.endCharacterNumber - 1
                    }
                }
            });
        }
    }
}