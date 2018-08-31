using System;
using System.Collections.Generic;
using KindleLiteratuhr.Common;

namespace KindleLiteratuhr.Core
{
    public class Text
    {
        public float SpaceWidth { get; }
        public List<Line> Lines { get; }
        private int currentLineNumber = 1;

        public Text(TimeData timeDate, float spaceWidth)
        {
            SpaceWidth = spaceWidth;
            Lines = new List<Line>();
        }

        public void AddLine(Line line)
        {
            Lines.Add(line);
        }

        public Line NewLine()
        {
            var line = new Line(currentLineNumber++, this);
            Lines.Add(line);

            return line;
        }
    }
}
