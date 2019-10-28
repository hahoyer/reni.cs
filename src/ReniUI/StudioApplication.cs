using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ReniUI
{
    public sealed class StudioApplication : ApplicationContext, IStudioApplication
    {
        readonly IFileOpenController FileOpenController = new FileOpenController(".");
        readonly List<Form> Children = new List<Form>();

        void IApplication.Register(Form child)
        {
            Children.Add(child);
            child.FormClosing += (a, s) => CheckedExit(child);
            child.Activated += (a, s) => OnActivated(child);
        }

        void OnActivated(Form child)
        {
            var editView = child as IEditView;
            if(editView != null)
                FileOpenController.FileName = editView.FileName;
        }

        void IStudioApplication.Exit() => ExitThread();

        void IStudioApplication.Open() => Open();

        void Open() => FileOpenController.QueryFileAndOpenIt(this);

        void CheckedExit(Form child)
        {
            Children.Remove(child);
            if(Children.Any(item => item.Visible))
                return;

            ExitThread();
        }

        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var editorViews = SystemConfiguration
                .ActiveFileNames
                .Select(file => new EditorView(file, this))
                .ToArray();

            if(editorViews.Any())
                foreach(var editorView in editorViews)
                    editorView.Run();
            else
            {
                Open();
                if(FileOpenController.FileName == null)
                    return;
            }

            Application.Run(this);
        }
    }

    interface IEditView
    {
        string FileName { get; }
    }
}