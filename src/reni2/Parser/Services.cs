using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Graphics;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;

namespace Reni.Parser
{
    public static class Services
    {
        public static PrioTable FormatPrioTable(this string text) { return PrioTable.FromText(text); }

        public static Image SyntaxGraph(this PrioTable prioTable, string code)
        {
            var syntax = Syntax(prioTable, code);
            return syntax == null ? new Bitmap(1, 1) : SyntaxDrawer.DrawBitmap(syntax);
        }

        public static IGraphTarget Syntax(this PrioTable prioTable, string code)
        {
            if(string.IsNullOrEmpty(code))
                return null;

            return (IGraphTarget) Position.Parse(new Source(code), new SimpleTokenFactory(prioTable),new ReniScanner());
        }

        public static string ToBase64(this Image image)
        {
            var ic = new ImageConverter();
            Tracer.Assert(image != null, () => "image != null");
            var convertTo = (byte[]) ic.ConvertTo(image, typeof(byte[]));
            Tracer.Assert(convertTo != null, () => "convertTo != null");
            return Convert.ToBase64String(convertTo, Base64FormattingOptions.InsertLineBreaks);
        }

        public static Image SaveToFile(this Image image, string fileName)
        {
            image.Save(fileName);
            return image;
        }
    }
}