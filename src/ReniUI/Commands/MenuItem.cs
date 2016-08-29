using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace ReniUI.Commands
{
    abstract class MenuItem : DumpableObject
    {
        internal string Name { get; }

        protected MenuItem(string name) { Name = name; }

        internal abstract System.Windows.Forms.MenuItem CreateMenuItem();
        internal abstract bool GetEnabled();
    }

    static class Command
    {
        public static ICommandHandler<EditorView> ListMembers { get; }
        = new CommandHandler<EditorView>(s => s.ListMembers(), s => true, "ListMembers");
        public static ICommandHandler<EditorView> IssuesView { get; }
        = new CommandHandler<EditorView>(s => s.Issues(), s => true, "Issues");
        public static ICommandHandler<IStudioApplication> Exit { get; }
        = new CommandHandler<IStudioApplication>(s => s.Exit(), s => true, "Exit");
        public static ICommandHandler<IStudioApplication> Open { get; }
        = new CommandHandler<IStudioApplication>(s => s.Open(), s => true, "Open");

        public static ICommandHandler<EditorView> FormatAll { get; }
        = new CommandHandler<EditorView>(s => s.FormatAll(), s => true, "FormatAll");
        public static ICommandHandler<EditorView> FormatSelection { get; }
        = new CommandHandler<EditorView>
            (s => s.FormatSelection(), s => s.HasSelection(), "FormatSelection");
    }

    sealed class CommandHandler<T> : DumpableObject, ICommandHandler<T>
    {
        Action<T> Click { get; }
        Func<T, bool> IsValid { get; }
        string Title { get; }

        public CommandHandler(Action<T> click, Func<T, bool> isValid, string title)
        {
            Click = click;
            IsValid = isValid;
            Title = title;
        }

        void ICommandHandler<T>.Click(T target) => Click(target);
        bool ICommandHandler<T>.IsValid(T target) => IsValid(target);

        string ICommandHandler<T>.Title => Title;
    }
}