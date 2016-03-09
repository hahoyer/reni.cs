using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ReniUI.Commands
{
    static class Extension
    {
        internal static MenuItem MenuItem<T>
            (this ICommandHandler<T> commandHandler, T target, string title = null)
            => new MenuEntry<T>(commandHandler, target, title);

        public static MainMenu CreateMainMenu(this EditorView target)
        {
            var m = new[]
            {
                new Menu("File")
                {
                    Entries = new[]
                    {
                        Command.Open.MenuItem(target),
                        Command.New.MenuItem(target.Master),
                        Command.Exit.MenuItem(target.Master)
                    }
                }       ,
                new Menu("Edit")
                {
                    Entries = new[]
                    {
                        Command.FormatAll.MenuItem(target),
                        Command.FormatSelection.MenuItem(target),
                        Command.Exit.MenuItem(target.Master)
                    }
                }

            };

            var menuStrip = new MainMenu();
            menuStrip.MenuItems.AddRange(m.Select(item => item.CreateMenuItem()).ToArray());
            return menuStrip;
        }

        public static void InvokeAsynchron(this Control target, Action action)
        {
            if (target.InvokeRequired)
            {
                target.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}