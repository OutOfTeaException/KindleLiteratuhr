using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KindleLiteratuhr.Common;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace KindleLiteratuhr.Core
{
    public partial class KindleImageGenerator
    {
        private static Font fontText;
        private static Font fontFooter;
        private const float FONTSIZE_FOOTER = 23;
        private readonly string csvFile;
        private readonly string outputDirectory;
        private static Object dictionaryLock = new Object();

        public KindleImageGenerator(string csvFile, string outputDirectory)
        {
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Install("Font/LinLibertine_RZah.ttf");
            fontText = fontFamily.CreateFont(55);

            var fontCollectionAuthor = new FontCollection();
            var fontFamilyAuthor = fontCollectionAuthor.Install("Font/LinLibertine_RZIah.ttf");
            fontFooter = fontFamilyAuthor.CreateFont(FONTSIZE_FOOTER);

            this.csvFile = csvFile;
            this.outputDirectory = outputDirectory;
        }

        public void GenerateImages()
        {
            Directory.CreateDirectory(outputDirectory);

            var csvReader = new CsvReader();
            var timeList = new Dictionary<string, int>();

            var startTime = DateTime.Now;
            Console.WriteLine("Start: {0}", startTime.ToLongTimeString());

            var timeDatas = csvReader.ReadFile(csvFile);

            Parallel.ForEach(timeDatas, (timeData, state, index) =>
             {
                 using (var image = GenerateImage(timeData))
                 {
                     // laufende Nummer pro Zeit ermitteln
                     int lfdNr = 0;
                     lock (dictionaryLock)
                     {
                         if (!timeList.ContainsKey(timeData.Time))
                         {
                             timeList.Add(timeData.Time, lfdNr);
                         }
                         else
                         {
                             lfdNr = timeList[timeData.Time];
                             lfdNr++;
                             timeList[timeData.Time] = lfdNr;
                         }
                     }

                     // Dateiname zusammenbauen
                     string filename = $"quote_{timeData.Time.Remove(2, 1)}_{lfdNr}.png";
                     string file = System.IO.Path.Combine(outputDirectory, filename);
                     Console.WriteLine($"{index,4} {timeData.Time}: {filename}");

                     SaveImage(image, file);
                 }
             });

            var endTime = DateTime.Now;
            Console.WriteLine("End: {0}", endTime.ToLongTimeString());
            Console.WriteLine("Duration: {0}", (endTime - startTime));
            Console.ReadLine();
        }

        private void SaveImage(Image<Rgba32> image, string file)
        {
            var pngEncoder = new PngEncoder
            {
                BitDepth = PngBitDepth.Bit8,
                ColorType = PngColorType.Grayscale
            };
            image.Save(file, pngEncoder);
        }

        private Image<Rgba32> GenerateImage(TimeData timeData)
        {
            if (timeData.TimeInTextPosition == -1)
            {
                throw new ArgumentException("Das Highlight konnte nicht im Text gefunden werden!");
            }

            var imageSize = new Rectangle(0, 0, 600, 800);
            var textSize = new Rectangle(30, 20, 545, 690);
            var footerSize = new Rectangle(30, imageSize.Height - textSize.Y, 580, 100);
            var footerLocation = new Point(footerSize.X, footerSize.Y);
               
            var textFont = new Font(fontText, 120);

            var footerText = CreateFooter(timeData, textSize);
            var footerOptions = new RendererOptions(fontFooter)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                WrappingWidth = 550,
                Origin = footerLocation
            };
            var footerGlyphs = TextBuilder.GenerateGlyphs(footerText.text, footerOptions);

            var image = new Image<Rgba32>(Configuration.Default, 600, 800, Rgba32.White);
            
            image.Mutate(ctx =>
            {
                DrawText(timeData, textFont, textSize, ctx);
                ctx.Fill(Rgba32.Black, footerGlyphs);
            });

            return image;
        }

        private (string text, int height) CreateFooter(TimeData timeData, Rectangle textSize)
        {
            string renderedText = timeData.Footer;
            SizeF footerSize;

            const double TEXT_HEIGHT = FONTSIZE_FOOTER + 7;

            var options = new RendererOptions(fontFooter)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                WrappingWidth = textSize.Width
            };

            footerSize = TextMeasurer.Measure(renderedText, options);

            if (footerSize.Height > TEXT_HEIGHT)
            {
                renderedText = timeData.Book + Environment.NewLine + timeData.Author;

                footerSize = TextMeasurer.Measure(renderedText, options);

                if (footerSize.Height > TEXT_HEIGHT * 2)
                {
                    renderedText = $"{timeData.Book}, {timeData.Author}";
                    footerSize = TextMeasurer.Measure(renderedText, options);
                }
            }

            return (renderedText, (int)footerSize.Height);
        }

        private void DrawText(TimeData timeData, Font baseFont, Rectangle textSize, IImageProcessingContext<Rgba32> image)
        {
            int fontSize = 120;
            var textGenerator = new TextGenererator();

            while (true)
            {
                Font font = new Font(baseFont, fontSize);
                Font fontBold = new Font(baseFont, fontSize, FontStyle.Bold);
                var options = new RendererOptions(font)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                float spaceWidth = TextMeasurer.Measure(" ", options).Width;
                float lineHeight = font.Size * 1.15f;

                var text = textGenerator.GenerateText(timeData, textSize.Width, font);
                float height =  text.Lines.Count() * lineHeight;
                float maxLineWidth = text.Lines.Select(l => l.Width).Max();

                // Solange die Font Groesse verringern, bis der Text passt.
                if (height < textSize.Height && maxLineWidth <= textSize.Width)
                {
                    float y = textSize.Y;

                    foreach (var line in text.Lines)
                    {
                        float x = textSize.X;

                        foreach (var word in line.Words)
                        {
                            var location = new PointF(x, y);
                            //System.Diagnostics.Debug.WriteLine($"{location}:'{word.Value}', {x + word.Width}");
                            
                            if (word.IsHighlight)
                            {
                                image.DrawText(word.Value, fontBold, Rgba32.Black, location);
                            }
                            else
                            {
                                image.DrawText(word.Value, font, Rgba32.Gray, location);
                            }
                            
                            x += word.Width;
                            if (word.SpaceAfter)
                            {
                                x += spaceWidth;
                            }
                        }

                        y += lineHeight;
                    }

                    return;
                }

                fontSize--;
            }
        }
    }
}
