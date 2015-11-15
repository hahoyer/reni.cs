using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using Reni.Parser;

namespace ReniTest.CompilationView
{
    sealed class DetailView : View
    {
        readonly CompileSyntax Syntax;

        public DetailView(CompileSyntax syntax)
            : base(syntax.GetType().PrettyName() + "-" + syntax.ObjectId)
        {
            Syntax = syntax;
            var resultCacheViews =
                Syntax
                    .ResultCache
                    .Select(item => new ResultCacheView(item).Control)
                    .ToArray();

            var client = new TableLayoutPanel
            {
                ColumnCount = resultCacheViews.Length,
                RowCount = 1
            };

            var styles = resultCacheViews
                .Select(item => new ColumnStyle(SizeType.Percent) {Width = 100/resultCacheViews.Length});
            foreach(var item in styles)
                client.ColumnStyles.Add(item);

            client.Controls.AddRange(resultCacheViews);

            Client = client;
        }
    }
}                                                           