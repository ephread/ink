using System.Collections.Generic;

namespace Ink {
    public struct Stats {

        public int words;
        public int knots;
        public int stitches;
        public int functions;
        public int choices;
        public int gathers;
        public int diverts;

        public static Stats Generate(Symbols symbols) {
            var stats = new Stats();

            // Count all the words across all strings
            stats.words = 0;
            foreach(var text in symbols.text) {

                var wordsInThisStr = 0;
                var wasWhiteSpace = true;
                foreach(var c in text.text) {
                    if( c == ' ' || c == '\t' || c == '\n' || c == '\r' ) {
                        wasWhiteSpace = true;
                    } else if( wasWhiteSpace ) {
                        wordsInThisStr++;
                        wasWhiteSpace = false;
                    }
                }

                stats.words += wordsInThisStr;
            }

            stats.knots = symbols.knots.Count;

            stats.functions = 0;
            foreach(var knot in symbols.knots)
                if (knot.isFunction) stats.functions++;

            stats.stitches = symbols.stitches.Count;

            stats.choices = symbols.choices.Count;

            // Skip implicit gather that's generated at top of story
            // (we know which it is because it isn't assigned debug metadata)
            stats.gathers = symbols.gathers.Count;

            // May not be entirely what you expect.
            // Does it nevertheless have value?
            // Includes:
            //  - DONE, END
            //  - Function calls
            //  - Some implicitly generated weave diverts
            // But we subtract one for the implicit DONE
            // at the end of the main flow outside of knots.
            stats.diverts = symbols.diverts.Count - 1;

            return stats;
        }

        public static Stats Generate(Ink.Parsed.Story story) {
            return Generate(Symbols.Generate(story));
        }

        public struct Symbols {
            public List<Parsed.Text> text;
            public List<Parsed.Knot> knots;
            public List<Parsed.Stitch> stitches;
            public List<Parsed.Choice> choices;
            public List<Parsed.Gather> gathers;
            public List<Parsed.Divert> diverts;

            public static Symbols Generate(Ink.Parsed.Story story) {
                var symbols = new Symbols();

                symbols.text = story.FindAll<Ink.Parsed.Text>();
                symbols.knots = story.FindAll<Ink.Parsed.Knot>();
                symbols.stitches = story.FindAll<Ink.Parsed.Stitch>();
                symbols.choices = story.FindAll<Ink.Parsed.Choice>();
                symbols.gathers = story.FindAll<Ink.Parsed.Gather>(g => g.debugMetadata != null);
                symbols.diverts = story.FindAll<Ink.Parsed.Divert>();

                return symbols;
            }
        }
    }
}