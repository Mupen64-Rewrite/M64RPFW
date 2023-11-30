using SkiaSharp;

namespace M64RPFW.ViewModels.Scripting.Graphics;

public static class TextLayout
{
    public static SKTextBlob LayoutText(string text, SKFont font, float width, SKTextAlign align)
    {
        var glyphs = font.Typeface.GetGlyphs(text);
        var lineHeight = font.GetFontMetrics(out var metrics);
        SKTextBlobBuilder builder = new SKTextBlobBuilder();

        // Measure the width of a tab (4 for monospaced fonts, 8 for proportional fonts)
        // float tabWidth;
        // {
        //     var tabTest = new string(' ', font.Typeface.IsFixedPitch ? 4 : 8);
        //     var testGlyphs = font.Typeface.GetGlyphs(tabTest);
        //     tabWidth = font.MeasureText(testGlyphs);
        // }

        int lineStart = 0;
        int lastSpace = -1;
        float yPos = lineHeight;

        void StartNewLine(int lineEnd)
        {
            // create new line
            var glyphSpan = new ReadOnlySpan<ushort>(glyphs, lineStart, lineEnd - lineStart);
            float lineWidth = font.MeasureText(glyphSpan);
            float xPos = align switch
            {
                SKTextAlign.Left => 0,
                SKTextAlign.Right => width - lineWidth,
                SKTextAlign.Center => (width - lineWidth) / 2,
                _ => 0, // default
            };
            builder.AddRun(glyphSpan, font, new SKPoint(xPos, yPos));
                    
            yPos += lineHeight;
            lineStart = lineEnd + 1;
            lastSpace = lineEnd;
        }
        
        for (int i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '\n':
                    StartNewLine(i);
                    break;
                case ' ':
                    if (font.MeasureText(new ReadOnlySpan<ushort>(glyphs, lineStart, i)) > width)
                        StartNewLine(lastSpace == lineStart - 1 ? i : lastSpace);
                    else
                        lastSpace = i;
                    break;
            }
        }
        StartNewLine(text.Length);

        var blob = builder.Build();
        return blob;
    }
}