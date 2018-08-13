using System;
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
                            Text = contents[2]
                                .Replace("\"\"\"", "\"")    // """ -> "
                                .Replace("\"\"", "\"")      // "" -> "
                                .Replace("''", "\""),       // '' -> "
                            Book = contents[3],
                            Author = contents[4]
                        };

            return times;
        }
    }
}
