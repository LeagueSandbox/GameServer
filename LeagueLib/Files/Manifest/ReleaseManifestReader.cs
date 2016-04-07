using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files.Manifest
{
    public class ReleaseManifestReader
    {
        private const int DirectoryEntrySize = sizeof(UInt32) * 5;
        private const int FileEntrySize = 44;

        public string FileLocation { get; private set; }
        
        private MemoryStream _stream;
        private BinaryReader _reader;

        private ReleaseManifest _manifest;

        private uint _directoryTableCount;
        private long _directoryTableDataOffset;

        private uint _fileTableCount;
        private long _fileTableDataOffset;

        private ReleaseManifestDirectoryDescriptor[] _directoryDescriptors;
        private ReleaseManifestDirectoryEntry[] _directoryTable;
        private ReleaseManifestDirectoryEntry[] _fileParentTable;
        private ReleaseManifestFileEntry[] _files;

        public ReleaseManifestReader(string path)
        {
            FileLocation = path;
        }

        public ReleaseManifest Read()
        {
            _manifest = new ReleaseManifest();
            _manifest.FileLocation = FileLocation;

            _stream = new MemoryStream(File.ReadAllBytes(FileLocation));
            _reader = new BinaryReader(_stream);

            DeserializeHeader();
            DeserializeSkipFileSystemBody();
            DeserializeStringTable();
            DeserializeFileSystemBody();

            return _manifest;
        }

        private void DeserializeHeader()
        {
            _manifest.Header = new ReleaseManifestHeader();
            _manifest.Header.Magic = _reader.ReadUInt32();
            _manifest.Header.FormatVersion = _reader.ReadUInt32();
            _manifest.Header.UnknownCount = _reader.ReadUInt32();
            _manifest.Header.EntityVersion = _reader.ReadUInt32();
        }

        private void DeserializeSkipFileSystemBody()
        {
            _directoryTableCount = _reader.ReadUInt32();
            _directoryTableDataOffset = _reader.BaseStream.Position;
            _reader.BaseStream.Position += DirectoryEntrySize * _directoryTableCount;

            _fileTableCount = _reader.ReadUInt32();
            _fileTableDataOffset = _reader.BaseStream.Position;
            _reader.BaseStream.Position += FileEntrySize * _fileTableCount;
        }

        private void DeserializeStringTable()
        {
            StringTable stringTable = new StringTable();
            stringTable.Count = _reader.ReadUInt32();
            stringTable.BlockSize = _reader.ReadUInt32();
            stringTable.Strings = new string[stringTable.Count];

            for (int i = 0; i < stringTable.Count; i++)
            {
                stringTable[i] = ReadString();
            }

            _manifest.Strings = stringTable;
        }

        private void DeserializeFileSystemBody()
        {
            _reader.BaseStream.Position = _directoryTableDataOffset;
            _directoryDescriptors = new ReleaseManifestDirectoryDescriptor[_directoryTableCount];
            _directoryTable = new ReleaseManifestDirectoryEntry[_directoryTableCount];
            _fileParentTable = new ReleaseManifestDirectoryEntry[_fileTableCount];

            for (int i = 0; i < _directoryTableCount; i++)
                _directoryDescriptors[i] = ReadDirectoryDescriptor();

            DeserializeTreeifyDirectoryDescriptor(0);
            _manifest.Root = _directoryTable[0];
            _manifest.Directories = _directoryTable;

            _reader.BaseStream.Position = _fileTableDataOffset;
            _files = new ReleaseManifestFileEntry[_fileTableCount];
            for (uint fileId = 0; fileId < _fileTableCount; fileId++)
            {
                ReleaseManifestFileEntryDescriptor fileDescriptor = ReadFileEntryDescriptor();
                _files[fileId] = new ReleaseManifestFileEntry(fileId, _manifest, fileDescriptor, _fileParentTable[fileId]);
            }

            _manifest.Files = _files;
        }

        private void DeserializeTreeifyDirectoryDescriptor(uint directoryId, ReleaseManifestDirectoryEntry parent = null)
        {
            ReleaseManifestDirectoryDescriptor directoryDescriptor = _directoryDescriptors[directoryId];
            ReleaseManifestDirectoryEntry directoryNode = new ReleaseManifestDirectoryEntry(directoryId, _manifest, directoryDescriptor, parent);
            _directoryTable[directoryId] = directoryNode;

            if (directoryDescriptor.FileCount != 0)
            {
                uint lastFileId = directoryDescriptor.FileStart + directoryDescriptor.FileCount;
                for (uint fileId = directoryDescriptor.FileStart; fileId < lastFileId; fileId++)
                    _fileParentTable[fileId] = directoryNode;
            }

            uint lastSubdirectoryId = directoryDescriptor.SubdirectoryStart + directoryDescriptor.SubdirectoryCount;
            for (uint subDirectoryId = directoryDescriptor.SubdirectoryStart; subDirectoryId < lastSubdirectoryId; subDirectoryId++)
                DeserializeTreeifyDirectoryDescriptor(subDirectoryId, directoryNode);
        }

        private ReleaseManifestDirectoryDescriptor ReadDirectoryDescriptor()
        {
            ReleaseManifestDirectoryDescriptor result = new ReleaseManifestDirectoryDescriptor();
            result.NameIndex = _reader.ReadUInt32();
            result.SubdirectoryStart = _reader.ReadUInt32();
            result.SubdirectoryCount = _reader.ReadUInt32();
            result.FileStart = _reader.ReadUInt32();
            result.FileCount = _reader.ReadUInt32();
            return result;
        }

        private ReleaseManifestFileEntryDescriptor ReadFileEntryDescriptor()
        {
            ReleaseManifestFileEntryDescriptor result = new ReleaseManifestFileEntryDescriptor();
            result.NameIndex = _reader.ReadUInt32();
            result.ArchiveId = _reader.ReadUInt32();
            result.ChecksumLow = _reader.ReadUInt64();
            result.ChecksumHigh = _reader.ReadUInt64();
            result.EntityType = _reader.ReadUInt32();
            result.DecompressedSize = _reader.ReadUInt32();
            result.CompressedSize = _reader.ReadUInt32();
            result.Checksum2 = _reader.ReadUInt32();
            result.PatcherEntityType = _reader.ReadUInt16();
            result.UnknownConstant1 = _reader.ReadByte();
            result.UnknownConstant2 = _reader.ReadByte();
            return result;
        }

        private string ReadString()
        {
            string result = "";
            int character = _stream.ReadByte();
            while (character > 0)
            {
                result += (char)character;
                character = _stream.ReadByte();
            }

            return result;
        }
    }
}
