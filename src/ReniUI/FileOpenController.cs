using System;
using System.Collections.Generic;
using System.Linq;

namespace ReniUI
{
    public sealed class FileOpenController : IFileOpenController
    {
        internal string FileName;

        string IFileOpenController.FileName
        {
            get { return FileName; }
            set { FileName = value; }
        }

        string IFileOpenController.CreateEmptyFile => "";

        public void OnOpen() => this.OnFileOpen("Reni file", "Reni files|*.reni|All files|*.*","reni");
    }
}