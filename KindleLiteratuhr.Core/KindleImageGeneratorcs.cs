using System;
using KindleLiteratuhr.Common;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace KindleLiteratuhr.Core
{
    public class KindleImageGenerator
    {
        private static Font fontText;
        private static Font fontFooter;
        private const float FONTSIZE_FOOTER = 23;
        private readonly string csvFile;
        private readonly string outputDirectory;

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
            // For Testing:
            var timeData = new TimeData
            {
                Time = "00:00",
                TimeInText = "midnight",
                Text = "The first night, as soon as the corporal midnight had conducted my uncle Toby up stairs, which was about 10 - Mrs. Wadman threw herself into her arm chair, and crossing her left knee with her right, which formed a resting-place for her elbow, she reclin'd her cheek upon the palm of her hand, and leaning forwards, ruminated until midnight upon both sides of the question.'",
                Author = "Laurence Sterne",
                Book = "The Life  and Opinions of Tristram Shandy"
            };

            using (var image = GenerateImage(timeData))
            {
                SaveImage(image, @"c:\temp\test_core.png");
            }

            /*
            var csvReader = new CsvReader();
            var timeList = new ConcurrentDictionary<string, int>();

            var startTime = DateTime.Now;
            Console.WriteLine("Start: {0}", startTime.ToLongTimeString());

            var timeDatas = csvReader.ReadFile(csvFile);

            Parallel.ForEach(timeDatas, (timeData, state, index) =>
            {
                using (var image = GenerateImage(timeData))
                {
                    // laufende Nummer pro Zeit ermitteln
                    int lfdNr = timeList.AddOrUpdate(timeData.Time, 0, (key, oldValue) => oldValue + 1);
                    // Dateiname zusammenbauen
                    string filename = $"quote_{timeData.Time.Remove(2, 1)}_{lfdNr}.png";
                    string file = Path.Combine(outputDirectory, filename);

                    SaveImage(image, file);
                }

                Console.WriteLine($"{index,4} {timeData.Time}: {filename}");
            });

            var endTime = DateTime.Now;
            Console.WriteLine("Ende: {0}", endTime.ToLongTimeString());
            Console.WriteLine("Dauer: {0}", (endTime - startTime));
            Console.ReadLine();
            */
        }

        private void SaveImage(Image<Rgba32> image, string file)
        {
            image.Save(file);
        }

        private Image<Rgba32> GenerateImage(TimeData timeData)
        {
            if (timeData.TimeInTextPosition == -1)
            {
                throw new ArgumentException("Das Highlight konnte nicht im Text gefunden werden!");
            }

            var imageSize = new Rectangle(0, 0, 600, 800);
            var textSize = new Rectangle(0, 0, 540, 690);
            var textMargin = new Point(30, 20);
            var footerLocation = new Point(textMargin.X + 10, imageSize.Height - textMargin.Y);

            float optimalFontSize = GetOptimalFontSize(timeData, textSize);
            var textFont = new Font(fontText, optimalFontSize);
            var textRendererOptions = new RendererOptions(textFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                WrappingWidth = textSize.Width,
                Origin = textMargin
            };

            var textGlyphs = TextBuilder.GenerateGlyphs(timeData.Text, textRendererOptions);
            var textGlyphsTime = TextBuilder.GenerateGlyphs(timeData.Text.Substring(timeData.TimeInTextPosition, timeData.TimeInText.Length), new RendererOptions(textFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                WrappingWidth = textSize.Width
                //Origin = new PointF(textGlyphs.Last().Bounds.Right, textGlyphs.Last().Bounds.Top)
            });

            var footerText = CreateFooter(timeData, textSize);
            var footerOptions = new RendererOptions(fontFooter)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                WrappingWidth = textSize.Width,
                Origin = footerLocation
            };
            var footerGlyphs = TextBuilder.GenerateGlyphs(footerText.text, footerOptions);

            var image = new Image<Rgba32>(Configuration.Default, 600, 800, Rgba32.White);
            
            image.Mutate(ctx => ctx
                .Fill(Rgba32.Gray, textGlyphs)
                //.Fill(Rgba32.Black, textGlyphsTime)
                .Fill(Rgba32.Black, footerGlyphs)
                );

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

        private float GetOptimalFontSize(TimeData timeData, Rectangle textSize)
        {
            float fontSize = 120;

            while (true)
            {
                var testFont = new Font(fontText, fontSize);
                var options = new RendererOptions(testFont)
                {
                    WrappingWidth = textSize.Width,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                var size = TextMeasurer.Measure(timeData.Text, options);

                if (size.Height < textSize.Height)
                {
                    return fontSize;
                }

                fontSize--;
            }
        }
    }
}
