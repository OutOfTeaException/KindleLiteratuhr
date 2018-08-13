using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;

namespace KindleLiteratuhr.Core
{
    class Program
    {
        private static Font font;
        private static Font fontAuthor;

        static void Main(string[] args)
        {
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Install("Font/LinLibertine_RZah.ttf");
            font = fontFamily.CreateFont(55);

            var fontCollectionAuthor = new FontCollection();
            var fontFamilyAuthor = fontCollectionAuthor.Install("Font/LinLibertine_RZIah.ttf");
            fontAuthor = fontFamilyAuthor.CreateFont(24);
            

            //String quote = "It was 11:55 a.m. on April 30.";
            String quote = @"... You had no reason to think the times important. Indeed how suspicious it would be if you had been completely accurate. ""Haven't I been?"" Not quite. It was five to seven that you talked to Wilkins. ""Another ten minutes.""";
            String highlight = "five to seven";
            String footer = "The Quiet American, Graham Greene";

            CreateImage(quote, highlight, footer);
        }

        private static void CreateImage(string quote, string highlight, string footer)
        {
            var authorOptions = new TextGraphicsOptions(true)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                WrapTextWidth = 500
            };

            var textGraphicOptions = new TextGraphicsOptions(true)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                WrapTextWidth = 560
            };

            using (Image<Rgba32> image = new Image<Rgba32>(600, 800))
            {
                var location = new PointF(80, image.Height - 25);

                image.Mutate(ctx => ctx
                   .Fill(Rgba32.White) // white background image
                   .ApplyScalingedText(font, quote, highlight, Rgba32.Gray, Rgba32.Black, 22, 60)
                   //.DrawText(textGraphicOptions, "Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins Wilkins", font, Rgba32.Gray, new PointF(22,22))
                   .DrawText(authorOptions, footer, fontAuthor, Rgba32.Black, location));

                image.Save(@"c:\temp\test_core.png");
            }
        }
    }
}
