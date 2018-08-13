using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KindleLiteratuhr.Common
{
    public class CsvReader
    {
        public IEnumerable<TimeData> ReadFile(string file)
        {
            var times = from line in File.ReadLines(file, Encoding.UTF8)
                        where line != ""
                        let contents = line.Split('|')
                        select new TimeData
                        {

                            Time = contents[0],
                            TimeInText = contents[1],
                            Text = contents[2].Modify(t =>
                           {
                               string text = t.Trim();
                               var sb = new StringBuilder(text);

                               sb.Replace("\"\"\"", "\"")    // """ -> "
                               .Replace("\"\"", "\"")      // "" -> "
                               .Replace("''", "\"");        // '' -> "

                               int pos = -1;
                               var quotes = new List<int>();
                               while ((pos = sb.ToString().IndexOf('"', pos + 1)) > -1)
                               {
                                   quotes.Add(pos);
                               }

                               if (quotes.Count % 2 == 0)
                               {
                                   for (int i = 0; i < quotes.Count; i += 2)
                                   {
                                       sb.Remove(quotes[i], 1);
                                       sb.Insert(quotes[i], "“");
                                       sb.Remove(quotes[i + 1], 1);
                                       sb.Insert(quotes[i + 1], "”");
                                   }
                               }
                               return sb.ToString();
                           }),
                            Book = contents[3].Trim(),
                            Author = contents[4].Trim()
                        };

            return times;
        }
    }
}
