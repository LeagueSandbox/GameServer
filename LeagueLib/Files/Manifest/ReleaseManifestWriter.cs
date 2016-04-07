using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files.Manifest
{
    public class ReleaseManifestWriter
    {
        MemoryStream _stream;
        BinaryWriter _writer;

        ReleaseManifest _manifest;

        public ReleaseManifestWriter(ReleaseManifest manifest)
        {
            _manifest = manifest;
        }

        public void Save(string path)
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
            Serialize();
            File.WriteAllBytes(path, _stream.ToArray());
        }

        public void Serialize()
        {
            SerializeHeader();
            SerializeDirectoryTable();
            SerializeFileTable();
            SerializeStringTable();
        }

        public void SerializeHeader()
        {
            _writer.Write(_manifest.Header.Magic);
            _writer.Write(_manifest.Header.FormatVersion);
            _writer.Write(_manifest.Header.UnknownCount);
            _writer.Write(_manifest.Header.EntityVersion);
        }

        public void SerializeDirectoryTable()
        {
            _writer.Write((uint)_manifest.Directories.Length);

            for (uint i = 0; i < _manifest.Directories.Length; i++)
                SerializeDirectory(_manifest.Directories[i]);
        }

        public void SerializeDirectory(ReleaseManifestDirectoryEntry directory)
        {
            _writer.Write(directory.NameStringTableIndex);
            _writer.Write(directory.SubdirectoryStart);
            _writer.Write(directory.SubdirectoryCount);
            _writer.Write(directory.FileStart);
            _writer.Write(directory.FileCount);
        }

        public void SerializeFileTable()
        {
            _writer.Write(_manifest.Files.Length);

            for (uint i = 0; i < _manifest.Files.Length; i++)
                SerializeFile(_manifest.Files[i]);
        }

        public void SerializeFile(ReleaseManifestFileEntry file)
        {
            _writer.Write(file.NameStringTableIndex);
            _writer.Write(file.ArchiveId);
            _writer.Write(file.ChecksumLow);
            _writer.Write(file.ChecksumHigh);
            _writer.Write(file.EntityType);
            _writer.Write(file.DecompressedSize);
            _writer.Write(file.CompressedSize);
            _writer.Write(file.Checksum2);
            _writer.Write(file.PatcherEntityType);
            _writer.Write(file.UnknownConstant1);
            _writer.Write(file.UnknownConstan2);
        }

        public void SerializeStringTable()
        {
            _writer.Write(_manifest.Strings.Count);
            _writer.Write(_manifest.Strings.BlockSize);

            for(int i = 0; i < _manifest.Strings.Count; i++)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(_manifest.Strings[i]);
                _writer.Write(bytes, 0, bytes.Length);
                _writer.Write((byte)0);
            }
        }
    }
}
