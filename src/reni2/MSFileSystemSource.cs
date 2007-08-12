using System.IO;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal class MSFileSystemSource : Source
    {
        private string _fullPathName;

        public MSFileSystemSource(string fullPathName)
        {
            _fullPathName = fullPathName;
        }

    }
}