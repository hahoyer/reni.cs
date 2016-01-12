using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using Reni;
using Reni.Context;

namespace ReniBrowser.CompilationView
{
    sealed class ResultCachesViewsPanel : DumpableObject, ViewExtension.IClickHandler
    {
        readonly IDictionary<ContextBase, ResultCache> Target;
        readonly SourceView Master;
        public readonly Control Client;

        public ResultCachesViewsPanel
            (IDictionary<ContextBase, ResultCache> target, SourceView master)
        {
            Target = target;
            Master = master;
            var selector = true.CreateLineupView
                (Target.Keys.Select(i => i.CreateLink(this)).ToArray());
            Client = false.CreateLineupView(selector, Target.First().CreateView(Master));
        }

        void ViewExtension.IClickHandler.Signal(object target)
        {
            Client.SuspendLayout();
            var item = Target.Single(i => i.Key == target);

            Client.Controls.RemoveAt(1);
            Client.Controls.Add(item.CreateView(Master));

            Client.ResumeLayout();
        }
    }
}