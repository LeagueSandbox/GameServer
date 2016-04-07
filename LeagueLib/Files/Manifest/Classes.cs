using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueLib.Files.Manifest
{
    public class ReleaseManifestHeader
    {
        public uint Magic { get; set; }
        public uint FormatVersion { get; set; }
        public uint UnknownCount { get; set; }
        public uint EntityVersion { get; set; }
    }

    public class ReleaseManifestDirectoryDescriptor
    {
        public uint NameIndex { get; set; }
        public uint SubdirectoryStart { get; set; }
        public uint SubdirectoryCount { get; set; }
        public uint FileStart { get; set; }
        public uint FileCount { get; set; }
    }

    public class ReleaseManifestDirectoryEntry
    {
        public uint Id { get; private set; }
        public ReleaseManifestDirectoryDescriptor Descriptor { get; private set; }
        public ReleaseManifest ReleaseManifest { get; private set; }
        public ReleaseManifestDirectoryEntry Parent { get; set; }
        public List<ReleaseManifestDirectoryEntry> Directories { get; set; }
        public List<ReleaseManifestFileEntry> Files { get; set; }

        private string _fullName;

        public ReleaseManifestDirectoryEntry(uint directoryId, ReleaseManifest manifest, ReleaseManifestDirectoryDescriptor descriptor, ReleaseManifestDirectoryEntry parent)
        {
            Id = directoryId;
            ReleaseManifest = manifest;
            Descriptor = descriptor;
            Parent = parent;
            if (parent != null)
                parent.Directories.Add(this);
            Directories = new List<ReleaseManifestDirectoryEntry>((int)descriptor.SubdirectoryCount);
            Files = new List<ReleaseManifestFileEntry>((int)descriptor.FileCount);
        }

        public string Name { get { return ReleaseManifest.Strings[Descriptor.NameIndex]; } }
        public string FullName
        {
            get
            {
                if (_fullName == null)
                {
                    if (Parent != null && !String.IsNullOrEmpty(Parent.FullName))
                        _fullName = Parent.FullName + "/" + Name;
                    else
                        _fullName = Name;
                }

                return _fullName;
            }
        }
        public uint NameStringTableIndex { get { return Descriptor.NameIndex; } }
        public uint SubdirectoryStart { get { return Descriptor.SubdirectoryStart; } }
        public uint SubdirectoryCount { get { return Descriptor.SubdirectoryCount; } }
        public uint FileStart { get { return Descriptor.FileStart; } }
        public uint FileCount { get { return Descriptor.FileCount; } }

        public ReleaseManifestFileEntry GetChildFileOrNull(string childName)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i].Name.Equals(childName, StringComparison.OrdinalIgnoreCase))
                    return Files[i];
            }

            return null;
        }

        public ReleaseManifestDirectoryEntry GetChildDirectoryOrNull(string childName)
        {
            for (int i = 0; i < Directories.Count; i++)
            {
                if (Directories[i].Name.Equals(childName, StringComparison.OrdinalIgnoreCase))
                    return Directories[i];
            }

            return null;
        }

        public IEnumerable<ReleaseManifestFileEntry> GetAllSubfiles()
        {
            IEnumerable<ReleaseManifestFileEntry> result = Files;
            for(int i = 0; i < Directories.Count; i++)
            {
                result = result.Concat(Directories[i].GetAllSubfiles());
            }
            return result;
        }
    }

    public class ReleaseManifestFileEntryDescriptor
    {
        public uint NameIndex { get; set; }
        public uint ArchiveId { get; set; }
        public ulong ChecksumLow { get; set; }
        public ulong ChecksumHigh { get; set; }
        public uint EntityType { get; set; }
        public uint DecompressedSize { get; set; }
        public uint CompressedSize { get; set; }
        public uint Checksum2 { get; set; }
        public ushort PatcherEntityType { get; set; }
        public byte UnknownConstant1 { get; set; }
        public byte UnknownConstant2 { get; set; }
    }

    public class ReleaseManifestFileEntry
    {
        public uint Id { get; private set; }
        public ReleaseManifest ReleaseManifest { get; private set; }
        public ReleaseManifestFileEntryDescriptor Descriptor { get; private set; }
        public ReleaseManifestDirectoryEntry Parent { get; private set; }

        private string _fullName;

        public ReleaseManifestFileEntry(uint fileId, ReleaseManifest releaseManifest, ReleaseManifestFileEntryDescriptor fileDescriptor, ReleaseManifestDirectoryEntry parent)
        {
            Id = fileId;
            ReleaseManifest = releaseManifest;
            Descriptor = fileDescriptor;
            Parent = parent;
            Parent.Files.Add(this);
        }

        public string Name { get { return ReleaseManifest.Strings[Descriptor.NameIndex]; } }
        public string FullName
        {
            get
            {
                if (_fullName == null)
                {
                    if (Parent != null && !String.IsNullOrEmpty(Parent.FullName))
                        _fullName = Parent.FullName + "/" + Name;
                    else
                        _fullName = Name;
                }

                return _fullName;
            }
        }
        public uint NameStringTableIndex { get { return Descriptor.NameIndex; } }
        public uint ArchiveId { get { return Descriptor.ArchiveId; } }
        public ulong ChecksumLow { get { return Descriptor.ChecksumLow; } }
        public ulong ChecksumHigh { get { return Descriptor.ChecksumHigh; } }
        public uint EntityType { get { return Descriptor.EntityType; } }
        public uint DecompressedSize { get { return Descriptor.DecompressedSize; } }
        public uint CompressedSize { get { return Descriptor.CompressedSize; } }
        public uint Checksum2 { get { return Descriptor.Checksum2; } }
        public ushort PatcherEntityType { get { return Descriptor.PatcherEntityType; } }
        public byte UnknownConstant1 { get { return Descriptor.UnknownConstant1; } }
        public byte UnknownConstan2 { get { return Descriptor.UnknownConstant2; } }
    }

    public class StringTable
    {
        public uint Count { get; set; }
        public uint BlockSize { get; set; }
        public string[] Strings { get; set; }

        public string this[int index]
        {
            get
            {
                return Strings[index];
            }
            set
            {
                Strings[index] = value;
            }
        }

        public string this[uint index]
        {
            get
            {
                return Strings[index];
            }
            set
            {
                Strings[index] = value;
            }
        }
    }
}
