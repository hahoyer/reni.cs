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

        public override Set<File> Files
        {
            get
            {
                DirectoryInfo thisDirectoryInfo = new DirectoryInfo(_fullPathName);
                if(!thisDirectoryInfo.Exists)
                    return new Set<File>();

                Set<File> result = new Set<File>();

                DirectoryInfo[] directoryInfos = thisDirectoryInfo.GetDirectories();
                foreach (DirectoryInfo directoryInfo in directoryInfos)
                    result.Add(Directory.CreateMSFileSystem(_fullPathName, directoryInfo.Name));

                FileInfo[] fileInfos = thisDirectoryInfo.GetFiles();
                foreach (FileInfo fileInfo in fileInfos)
                    result.Add(DataFile.CreateMSFileSystem(_fullPathName, fileInfo.Name));

                return result;
            }
        }
    }
}