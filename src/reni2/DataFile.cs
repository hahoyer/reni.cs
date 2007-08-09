namespace HWFileSystem
{
    internal class DataFile : File
    {
        private DataFile(string name)
            : base(name)
        {
        }

        static public DataFile CreateMSFileSystem(string rootDirName, string name)
        {
            return new DataFile(name);
        }
    }
}