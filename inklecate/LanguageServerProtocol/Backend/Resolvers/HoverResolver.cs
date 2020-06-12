using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Helpers;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    public class HoverResolver: IHoverResolver
    {
        private readonly ISymbolResolver _symbolResolver;
        private readonly IWorkspaceFileHandler _fileHandler;

        public HoverResolver(
            ISymbolResolver symbolResolver,
            IWorkspaceFileHandler fileHandler)
        {
            _symbolResolver = symbolResolver;
            _fileHandler = fileHandler;
        }

        public Hover HoverForSymbolAt(Position position, Uri file, CancellationToken cancellationToken)
        {
            var parsedObject = _symbolResolver.SymbolAt(position, file, cancellationToken);

            if (cancellationToken.IsCancellationRequested) return null;

            if (parsedObject == null)
            {
                return null;
            }

            if (parsedObject is Parsed.FunctionCall functionCall)
            {
                if (functionCall.name != null)
                {
                    switch (functionCall.name)
                    {
                        case "CHOICE_COUNT":
                            return HoverFromMessage("`CHOICE_COUNT` returns the number of options created so far in the current chunk.");
                        case "TURNS":
                            return HoverFromMessage("`TURNS` returns the number of game turns since the game began.");
                        case "TURNS_SINCE":
                            return HoverFromMessage("`TURNS_SINCE` returns the number of moves (formally, player inputs) since a particular knot/stitch was last visited.");
                        case "RANDOM":
                            return HoverFromMessage("`RANDOM(min, max)` Ink can generate random integers if required using the `RANDOM` function. `RANDOM` is authored to be like a dice, so the min and max values are both inclusive.");
                        case "SEED_RANDOM":
                            return HoverFromMessage("`SEED_RANDOM` seeds the random number generator manually. For testing purposes, it's often useful to fix the random number generator so ink will produce the same outcomes every time you play. You can do this by _seeding_ the random number system.");
                        case "READ_COUNT":
                            return HoverFromMessage("`CHOICE_COUNT` returns the number of options created so far in the current chunk.");
                        case "LIST_VALUE":
                            return HoverFromMessage("`LIST_VALUE` returns the numerical value associated with the list. Note the first value in a list has the value 1, and not the value 0.");
                        case "LIST_RANDOM":
                            return HoverFromMessage("`LIST_RANDOM` returns a random item from the list.");
                        case "LIST_MIN":
                            return HoverFromMessage("`LIST_MIN` return the item with the smalest value. Note that it ignores items considered pout of the list.");
                        case "LIST_MAX":
                            return HoverFromMessage("`LIST_MAX` return the item with the largest value. Note that it ignores items considered pout of the list.");
                        case "LIST_COUNT":
                            return HoverFromMessage("`LIST_COUNT` returns the number of items in the list. Note that it ignores items considered pout of the list.");
                        case "LIST_ALL":
                            return HoverFromMessage("`LIST_ALL` returns all items from the list regardless of whether they are considered _in_ or _out_.");
                        case "LIST_INVERT":
                            return HoverFromMessage("`LIST_INVERT` invert the list (i. e., goes through the accommodation in/out name-board and flipping every switch to the opposite of what it was before. Note that LIST_INVERT on an empty list will return a null value, if the game doesn't have enough context to know what invert.");
                        case "LIST_RANGE":
                            return HoverFromMessage("`LIST_RANGE(list_name, min_value, max_value)` returns a _slice_ of the full list");
                        case "POW":
                            return HoverFromMessage("`POW(a, b)` returns the a raised to the power of b.");
                        case "FLOOR":
                            return HoverFromMessage("`FLOOR` returns the largest integer less than or equal to a given number.");
                        case "CEILING":
                            return HoverFromMessage("`CEILING` rounds a number up to the next largest integer.");
                        case "INT":
                            return HoverFromMessage("`INT` casts a value into a integer.");
                        case "FLOAT":
                            return HoverFromMessage("`FLOAT` casts a value into a floating-point number.");
                        case "MIN":
                            return HoverFromMessage("`MIN(a, b)` returns the lowest number between `a` and `b`.");
                        case "MAX":
                            return HoverFromMessage("`MAX(a, b)` returns the largest number between `a` and `b`.");
                        default: break;
                    }
                }

                return null;
            }

            if (parsedObject is Parsed.Sequence sequence)
            {
                var messages = new List<String>();

                if ((sequence.sequenceType & Ink.Parsed.SequenceType.Once) > 0)
                {
                    messages.Add(
                        "A **once-only (!)** alternative displays nothing when its run out of new content to display. (You can think of a once-only alternative as a sequence with a blank last entry.");
                }

                if ((sequence.sequenceType & Ink.Parsed.SequenceType.Cycle) > 0)
                {
                    messages.Add("A **cycle (&)** alternatives loop its content.");
                }

                if ((sequence.sequenceType & Ink.Parsed.SequenceType.Stopping) > 0)
                {
                    messages.Add("A **sequence** (or \"stopping blocks\") is a set of alternatives that tracks how many times its been seen, and each time, shows the next element along. When it runs out of new content it continues the show the final element.");
                }

                if ((sequence.sequenceType & Ink.Parsed.SequenceType.Shuffle) > 0)
                {
                    messages.Add("A **shuffle (~)** produces randomised output.");
                }

                return HoverFromMessages(messages);
            }

            return null;
        }

        private Hover HoverFromMessage(string message)
        {
            return HoverFromMessages(new List<string> { message });
        }

        private Hover HoverFromMessages(List<string> messages)
        {
            var markedStrings = messages.Select(messages => new MarkedString(messages));
            return new Hover() {
                Contents = new MarkedStringsOrMarkupContent(markedStrings)
            };
        }
    }
}