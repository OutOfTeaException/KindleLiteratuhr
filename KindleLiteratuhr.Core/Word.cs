using System.Diagnostics;

namespace KindleLiteratuhr.Core
{
    [DebuggerDisplay("{Value}")]
    public class Word
    {
        public string Value { get; }

        public float Width { get; set; }

        public bool IsHighlight { get; set; } = false;

        public bool SpaceAfter { get; set; } = false;

        public Line Line { get; set; }

        public Word(string value)
        {
            Value = value;
        }
    }
}
