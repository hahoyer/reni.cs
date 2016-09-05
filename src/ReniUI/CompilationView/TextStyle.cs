using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;
using ReniUI.Classification;
using ScintillaNET;

namespace ReniUI.CompilationView
{
    sealed class TextStyle : EnumEx
    {
        static int _nextId;
        [UsedImplicitly]
        public static readonly TextStyle Default = new TextStyle(Color.Black);
        [UsedImplicitly]
        public static readonly TextStyle KeyWord = new TextStyle(Color.Blue);
        [UsedImplicitly]
        public static readonly TextStyle BraceLikeKeyWord = new TextStyle(Color.Blue, isBold: true);
        [UsedImplicitly]
        public static readonly TextStyle Brace = new TextStyle(Color.Black, isBold: true);
        [UsedImplicitly]
        public static readonly TextStyle Comment = new TextStyle(Color.Green);
        [UsedImplicitly]
        public static readonly TextStyle Number = new TextStyle(Color.Purple);
        [UsedImplicitly]
        public static readonly TextStyle Text = new TextStyle(Color.Red);
        [UsedImplicitly]
        public static readonly TextStyle Error = new TextStyle
        (
            Color.Red,
            isItalic: true,
            isUnderlined: true);

        [UsedImplicitly]
        public static TextStyle From(Token token, CompilerBrowser compiler)
        {
            try
            {
                if(token.IsError)
                    return Error;
                if(token.IsBraceLike && compiler.FindAllBelongings(token).Skip(1).Any())
                    return token.IsBrace ? Brace : BraceLikeKeyWord;
                if(token.IsKeyword)
                    return KeyWord;
                if(token.IsComment || token.IsLineComment)
                    return Comment;
                if(token.IsNumber)
                    return Number;
                if(token.IsText)
                    return Text;

                return Default;
            }
            catch(Exception)
            {
                return Error;
            }
        }

        public static implicit operator int(TextStyle v) => v.Id;
        public static IEnumerable<TextStyle> All => AllInstances<TextStyle>();

        readonly int Id;
        readonly Color ForeColor;
        readonly bool IsItalic;
        readonly bool IsBold;
        readonly Color? BackColor;
        readonly bool IsUnderlined;

        TextStyle
        (
            Color foreColor,
            Color? backColor = null,
            bool isBold = false,
            bool isItalic = false,
            bool isUnderlined = false)
        {
            ForeColor = foreColor;
            BackColor = backColor;
            IsBold = isBold;
            IsItalic = isItalic;
            IsUnderlined = isUnderlined;
            Id = _nextId++;
        }

        internal void Config(Style style)
        {
            style.Font = "Lucida Console";
            style.SizeF = 8;
            style.ForeColor = ForeColor;
            if(BackColor != null)
                style.BackColor = BackColor.Value;
            style.Italic = IsItalic;
            style.Bold = IsBold;
            style.Underline = IsUnderlined;
        }
    }
}