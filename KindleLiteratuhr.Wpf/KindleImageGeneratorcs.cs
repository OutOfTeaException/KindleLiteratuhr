using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KindleLiteratuhr.Common;

namespace KindleLiteratuhr.Wpf
{
    public class KindleImageGenerator
    {
        private static Typeface font;
        private const double FONTSIZE_FOOTER = 23;
        private static readonly Brush COLOR_TEXT = Brushes.Gray;
        private static readonly Brush COLOR_FOOTER = Brushes.Black;
        private readonly string csvFile;
        private readonly string outputDirectory;

        public KindleImageGenerator(string csvFile, string outputDirectory)
        {
            FontFamily fontFamily = new FontFamily("Linux Libertine");
            font = new Typeface(fontFamily, FontStyles.Normal, FontWeights.SemiBold, new FontStretch());
            this.csvFile = csvFile;
            this.outputDirectory = outputDirectory;
        }

        public void GenerateImages()
        {
            // For Testing:
            /*var timeData = new TimeData
            {
                Time = "00:00",
                TimeInText = "midnight",
                Text = "The first night, as soon as the corporal had conducted my uncle Toby up stairs, which was about 10 - Mrs. Wadman threw herself into her arm chair, and crossing her left knee with her right, which formed a resting-place for her elbow, she reclin'd her cheek upon the palm of her hand, and leaning forwards, ruminated until midnight upon both sides of the question.'",
                Author = "Laurence Sterne ",
                Book = "The Life and Opinions of Tristram Shandy, Gentleman "
            };

            var image = GenerateImage(timeData);
            SaveImage(image, @"c:\temp\test.png");*/


            var csvReader = new CsvReader();
            int count = 1;

            var timeList = new Dictionary<string, int>();

            foreach (var timeData in csvReader.ReadFile(csvFile))
            {
                var image = GenerateImage(timeData);

                int lfdNr = 0;
                if (!timeList.ContainsKey(timeData.Time))
                {
                    timeList.Add(timeData.Time, 1);
                }
                else
                {
                    lfdNr = timeList[timeData.Time];
                    lfdNr++;
                    timeList[timeData.Time] = lfdNr;
                }

                string filename = $"quote_{timeData.Time.Remove(2, 1)}_{lfdNr}.png";
                string file = Path.Combine(outputDirectory, filename);

                SaveImage(image, file);

                Console.WriteLine($"{count++} {timeData.Time}");
            }
        }

        private void SaveImage(BitmapSource image, string file)
        {
            // Encode the RenderBitmapTarget as a PNG file.
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(image));
            using (Stream fileStream = File.Create(file))
            {
                pngEncoder.Save(fileStream);
            }
        }

        private RenderTargetBitmap GenerateImage(TimeData timeData)
        {
            if (timeData.TimeInTextPosition == -1)
            {
                throw new ArgumentException("Das Highlight konnte nicht im Text gefunden werden!");
            }

            var imageSize = new Rect(0, 0, 600, 800);
            var textSize = new Rect(0, 0, 540, 690);
            var textMargin = new Point(30, 20);
            double optimalFontSize = GetOptimalFontSize(timeData, textSize);

            FormattedText text = CreateFormattedText(timeData, textSize, optimalFontSize);
            FormattedText footerText = CreateFormattedFooter(timeData, textSize);

            var drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // Hintergrund weiß
                drawingContext.DrawRectangle(Brushes.White, new Pen(), imageSize);
                // Zitat
                drawingContext.DrawText(text, textMargin);
                // Buch und Autor
                drawingContext.DrawText(footerText, new Point(textMargin.X, imageSize.Height - textMargin.Y - footerText.Height));
            }

            // The BitmapSource that is rendered with a Visual.
            var image = new RenderTargetBitmap((int)imageSize.Width, (int)imageSize.Height, 96, 96, PixelFormats.Pbgra32);
            image.Render(drawingVisual);

            return image;
        }

        private FormattedText CreateFormattedText(TimeData timeData, Rect textSize, double fontSize)
        {
            FormattedText text = new FormattedText(timeData.Text, new CultureInfo("en-us"), FlowDirection.LeftToRight, font, fontSize, COLOR_TEXT);
            text.SetForegroundBrush(Brushes.Black, timeData.TimeInTextPosition, timeData.TimeInText.Length);
            text.SetFontWeight(FontWeights.Bold, timeData.TimeInTextPosition, timeData.TimeInText.Length);
            text.Trimming = TextTrimming.None;
            text.MaxTextWidth = textSize.Width;

            return text;
        }

        private FormattedText CreateFormattedFooter(TimeData timeData, Rect textSize)
        {
            const double TEXT_HEIGHT = FONTSIZE_FOOTER + 2;
            var footerText = new FormattedText(timeData.Footer, new CultureInfo("en-us"), FlowDirection.LeftToRight, font, FONTSIZE_FOOTER, COLOR_FOOTER);
            footerText.MaxTextWidth = textSize.Width;
            footerText.TextAlignment = TextAlignment.Right;
            footerText.SetFontStyle(FontStyles.Italic);

            if (footerText.Height > TEXT_HEIGHT)
            {
                string seperatedFooter = timeData.Book + Environment.NewLine + timeData.Author;
                footerText = new FormattedText(seperatedFooter, new CultureInfo("en-us"), FlowDirection.LeftToRight, font, FONTSIZE_FOOTER, COLOR_FOOTER);
                footerText.MaxTextWidth = textSize.Width;
                footerText.TextAlignment = TextAlignment.Right;
                footerText.SetFontStyle(FontStyles.Italic);

                if (footerText.Height > TEXT_HEIGHT * 2)
                {
                    string unifiedFooter = $"{timeData.Book}, {timeData.Author}";
                    footerText = new FormattedText(unifiedFooter, new CultureInfo("en-us"), FlowDirection.LeftToRight, font, FONTSIZE_FOOTER, COLOR_FOOTER);
                    footerText.MaxTextWidth = textSize.Width;
                    footerText.TextAlignment = TextAlignment.Right;
                    footerText.SetFontStyle(FontStyles.Italic);
                }
            }

            return footerText;
        }

        private double GetOptimalFontSize(TimeData timeData, Rect textSize)
        {
            double fontSize = 120;
            FormattedText formattedText = CreateFormattedText(timeData, textSize, fontSize);

            while (true)
            {
                formattedText.SetFontSize(fontSize);

                if (formattedText.Height < textSize.Height)
                {
                    return fontSize;
                }

                fontSize--;
            }
        }
    }
}
