using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace KindleLiteratuhr.Core
{
    public static class ImageExtensions
    {
        public static IImageProcessingContext<TPixel> ApplyScalingedText<TPixel>(this IImageProcessingContext<TPixel> processingContext, Font font, string text, string highlight,
                                                                                TPixel color, TPixel colorHighlight, float padding, int maxFontSize)
            where TPixel : struct, IPixel<TPixel>
        {
            return processingContext.Apply(image =>
            {
                float targetWidth = image.Width - (padding * 2);
                float targetHeight = image.Height - (padding * 2);
                float targetMinHeight = image.Height - (padding * 3) - 100; // must be with in a margin width of the target height

                // now we are working in 2 dimensions at once and can't just scale because it will cause the text to 
                // reflow we need to just try multiple times
                var scaledFont = font;
                SizeF s = new SizeF(float.MaxValue, float.MaxValue);

                float scaleFactor = (scaledFont.Size / 2);// everytime we change direction we half this size
                int trapCount = (int)scaledFont.Size * 2;
                if (trapCount < 10)
                {
                    trapCount = 10;
                }

                bool isTooSmall = false;

                while ((s.Height > targetHeight || s.Height < targetMinHeight) && trapCount > 0)
                {
                    if (s.Height > targetHeight)
                    {
                        if (isTooSmall)
                        {
                            scaleFactor = scaleFactor / 2;
                        }

                        scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
                        isTooSmall = false;
                    }

                    if (s.Height < targetMinHeight)
                    {
                        if (!isTooSmall)
                        {
                            scaleFactor = scaleFactor / 2;
                        }
                        scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
                        isTooSmall = true;
                    }
                    trapCount--;

                    s = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        WrappingWidth = targetWidth
                    });
                }

                var location = new PointF(padding, padding);
                var textGraphicOptions = new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    WrapTextWidth = targetWidth
                };

                if (scaledFont.Size > maxFontSize)
                {
                    scaledFont = new Font(scaledFont, maxFontSize);
                }

                image.Mutate(ctx =>
                {
                    foreach (string word in text.Split(new[] { ' ' }, options: System.StringSplitOptions.None))
                    {
                        var wordSize = TextMeasurer.Measure(word, new RendererOptions(scaledFont)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top
                            //WrappingWidth = targetWidth
                        });

                        // Wort zu langen. Umbrechen.
                        if (location.X + wordSize.Width > targetWidth)
                        {
                            //location = new PointF(padding, location.Y + wordSize.Height);
                            location = new PointF(padding, location.Y + scaledFont.Size);
                        }

                        foreach (char c in word + " ")
                        {
                            ctx.DrawText(textGraphicOptions, c.ToString(), scaledFont, color, location);

                            var charSize = TextMeasurer.Measure(c.ToString(), new RendererOptions(scaledFont)
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top
                            });

                            location = new PointF(location.X + charSize.Width, location.Y);
                        }
                    }
                    //ctx.DrawText(textGraphicOptions, text, scaledFont, color, location);
                    //ctx.DrawText(textGraphicOptions, highlight, scaledFont, colorHighlight, location);
                });
            });
        }
    }
}
