using System.Collections.Generic;
using System.Diagnostics;

namespace KindleLiteratuhr.Core
{
    [DebuggerDisplay("No: {Number} Width: {Width}")]
    public class Line
    {
        public Text Text { get; }

        public int Number { get; }
        public List<Word> Words { get; set; }

        public float Width
        {
            get
            {
                float width = 0;
                for (int i = 0; i < Words.Count; i++)
                {
                    var word = Words[i];
                    width += word.Width;

                    if (word.SpaceAfter && i + 1 != Words.Count)
                    {
                        width += Text.SpaceWidth;
                    }
                }

                return width;
            }
        }

        public Line(int number, Text text)
        {
            Number = number;
            Text = text;
            Words = new List<Word>();
        }
    }
}
