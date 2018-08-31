using KindleLiteratuhr.Common;
using SixLabors.Fonts;
using System.Linq;
using System.Text.RegularExpressions;
using static MoreLinq.Extensions.PairwiseExtension;

namespace KindleLiteratuhr.Core
{
    public class TextGenererator
    {
        public Text GenerateText(TimeData timeData, float maxWidth, Font font)
        {
            Text text;
            bool noBreak = false;

            var options = new RendererOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            float spaceWidth = TextMeasurer.Measure(" ", options).Width;
            text = new Text(timeData, spaceWidth);

            Word[] words = SplitWords(timeData);
            Line currentLine = text.NewLine();

            // Measure the length of all words
            foreach (var word in words)
            {
                word.Width = TextMeasurer.Measure(word.Value, options).Width;
            }

            for (int i = 0; i < words.Length; i++)
            {
                // Arrange the words in lines
                var wordBefore = i == 0 ? null : words[i - 1];
                var word = words[i];
                var wordAfter = i == words.Length - 1 ? null : words[i + 1];
                noBreak = false;
                float widthToMeasure = word.Width;

                // Sonderlocke für Zeichen vor den Highlights
                if (wordAfter != null && !word.IsHighlight && wordAfter.IsHighlight && !word.SpaceAfter)
                {
                    // Force line break if line too long.
                    widthToMeasure += wordAfter.Width;
                }

                // Sonderlocke nach Highlight
                if (wordBefore != null && wordBefore.IsHighlight && !word.IsHighlight && !wordBefore.SpaceAfter)
                {
                    noBreak = true;
                }

                // Line too wide. Do a line break.
                if (!noBreak && currentLine.Width + widthToMeasure > maxWidth - spaceWidth)
                {
                    currentLine = text.NewLine();
                }

                currentLine.Words.Add(word);
                word.Line = currentLine;
            }

            return text;
        }

        /// <summary>
        /// Splits the text by space.
        /// </summary>
        /// <param name="timeData"></param>
        /// <returns></returns>
        private Word[] SplitWords(TimeData timeData)
        {
            string textBefore = timeData.Text.Substring(0, timeData.TimeInTextPosition);
            string textAfter = timeData.Text.Substring(timeData.TimeInTextPosition + timeData.TimeInText.Length);

            // Split Text bei space (include the space in the results)
            var wordsBefore = Regex.Split(textBefore, "( )").Where(w => w != "")
                .Select(w => new Word(w));
            var wordsAfter = Regex.Split(textAfter, "( )").Where(w => w != "")
                .Select(w => new Word(w));
            var wordsTime = Regex.Split(timeData.TimeInText, "( )").Where(w => w != "")
                .Select(w => new Word(w) { IsHighlight = true });

            var wordsCombinded = wordsBefore.Concat(wordsTime).Concat(wordsAfter).ToArray();

            // Mark words wich have a succeeding space. Remove the space-words afterwards.
            var words = wordsCombinded
                .Prepend(new Word(null))
                .Pairwise((a, b) =>
                {
                    if (b.Value == " ")
                    {
                        a.SpaceAfter = true;
                    }

                    return b;
                })
            .Where(w => w.Value != " ")
            .ToArray();

            return words;
        }
    }
}
