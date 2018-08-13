using System;

namespace KindleLiteratuhr.Common
{
    public class TimeData
    {
        public string Time { get; set; }
        public string TimeInText { get; set; }
        public string Text { get; set; }
        public string Book { get; set; }
        public string Author { get; set; }

        public int TimeInTextPosition => Text.ToLowerInvariant().IndexOf(TimeInText.ToLowerInvariant());

        public string Footer => $"{Book}, {Author}"; 
    }
}
