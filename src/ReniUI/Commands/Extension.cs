using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ReniUI.Commands
{
    static class Extension
    {
        internal static MenuItem MenuItem<T>
            (this T target, ICommandHandler<T> commandHandler, string title = null)
            => new MenuEntry<T>(commandHandler, target, title);

        internal static IEnumerable<Menu> Menus(this EditorView target)
            => new[]
            {
                new Menu("File")
                {
                    Entries = new[]
                    {
                        target.Master.MenuItem(Command.Open),
                        target.Master.MenuItem(Command.New),
                        target.Master.MenuItem(Command.Exit)
                    }
                },
                new Menu("Edit")
                {
                    Entries = new[]
                    {
                        target.MenuItem(Command.FormatAll),
                        target.MenuItem(Command.FormatSelection),
                        new Menu("Intellisense")
                        {
                            Entries = new[] {target.MenuItem(Command.ListMembers)}
                        }
                    }
                },
                new Menu("View")
                {
                    Entries = new[]
                    {
                        target.MenuItem(Command.IssuesView)
                    }
                }
            };

        public static void InvokeAsynchron(this Control target, Action action)
        {
            if(target.InvokeRequired)
                target.Invoke(action);
            else
                action();
        }
    }
}