using System;
using System.Collections.Generic;
using System.Linq;

namespace ReniUI
{
    sealed class FileOpenController : IFileOpenController
    {
        internal string FileName;
        readonly string DefaultDirectory;
        internal FileOpenController(string defaultDirectory) { DefaultDirectory = defaultDirectory; }

        string IFileOpenController.FileName { get { return FileName; } set { FileName = value; } }

        string IFileOpenController.CreateEmptyFile => "";

        string IFileOpenController.DefaultDirectory => DefaultDirectory;

        public void OnOpen()
            => this.OnFileOpen("Reni file", "Reni files|*.reni|All files|*.*", "reni");
    }
}