using System.IO;

namespace LeagueSandbox.GameServer.Content
{
    public class FileEntry
    {
        public string Name { get; }
        public string FullPath { get; }

        private byte[] _data;

        public FileEntry(string name, string fullPath, Stream stream)
        {
            Name = name;
            FullPath = fullPath;
            using (stream)
            {
                _data = new byte[stream.Length];
                stream.Read(_data, 0, (int)stream.Length);
            }
        }

        public MemoryStream ReadFile()
        {
            return new MemoryStream(_data);
        }

        public override string ToString()
        {
            return FullPath;
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }
    }
}