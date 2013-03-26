using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Provider;
using System.Text;
using ClrPlus.Core.Exceptions;
using ClrPlus.Powershell.Provider.Base;
using DiscUtils;
using DiscUtils.Partitions;

namespace VHDProvider
{
    class VHDLoc : Location
    {
        private readonly DiscFileSystemInfo _FSI;
        private readonly PartitionInfo _drivePart;
        private readonly VHDDriveInfo _driveInfo;

        #region Constructors
        public VHDLoc(VHDDriveInfo Drive, DiscFileSystemInfo Location)
        {
            _driveInfo = Drive;
            _FSI = Location;
        }

        public VHDLoc(VHDDriveInfo Drive, string Location)
        {
            _driveInfo = Drive;
            Drive.
            _FSI = Location;
        }


        #endregion

        public override IEnumerable<ILocation> GetDirectories(bool recurse)
        {
            if (!_FSI.Attributes.HasFlag(FileAttributes.Directory))
                return Enumerable.Empty<VHDLoc>();

            var Dir = _FSI as DiscDirectoryInfo;
            if (Dir == null)
                return Enumerable.Empty<VHDLoc>();

            return Dir.GetDirectories("*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
               .Select(each => new VHDLoc(_driveInfo, each));

        }

        public override IEnumerable<ILocation> GetFiles(bool recurse)
        {
            var Dir = _FSI as DiscDirectoryInfo;
            if (Dir == null)
            {
                var file = _FSI as DiscFileInfo;
                if (file == null)
                    return Enumerable.Empty<VHDLoc>();
                var ret = new List<VHDLoc>();
                ret.Add(this);
                return ret;
            }

            return Dir.GetFiles("*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
               .Select(each => new VHDLoc(_driveInfo, each));

        }

        public override void Delete(bool recurse)
        {
            _FSI.Delete();
        }

        public override Stream Open(FileMode mode)
        {
            var file = _FSI as DiscFileInfo;
            if (file == null)
                throw new ClrPlusException("Unable to open stream:  Not a file");
            return file.Open(mode);
        }

        public override ILocation GetChildLocation(string relativePath)
        {
            return new VHDLoc(_driveInfo, _FSI.FileSystem.GetFileSystemInfo(System.IO.Path.Combine(_FSI.FullName, relativePath)));
        }

        public override IContentReader GetContentReader()
        {
            return new ContentReader(Open(FileMode.Open), Length);
        }

        public override IContentWriter GetContentWriter()
        {
            return new UniversalContentWriter(Open(FileMode.Create));
        }

        public override void ClearContent()
        {
            if (IsFile) 
                Open(FileMode.Truncate).Close();
        }

        public override string Name
        {
            get { return _FSI.Name; }
        }

        public override string AbsolutePath
        {
            get { return _driveInfo.Name + ":\\" + _FSI.FullName; }
        }

        public override string Url
        {
            get { return String.Empty; } // TODO: Verify that this is not needed.
        }

        public override string Type
        {
            get { return IsFile ? "<file>" : IsFileContainer ? "<dir>" : "<?>"; }
        }

        public override long Length
        {
            get
            {
                var file = _FSI as DiscFileInfo;
                if (file != null)
                    return file.Length;
                return -1;
            }
        }

        public override DateTime TimeStamp
        {
            get
            {
                return DateTime.Compare(_FSI.CreationTimeUtc, _FSI.LastAccessTimeUtc) < 0
                        ? DateTime.Compare(_FSI.LastAccessTimeUtc, _FSI.LastWriteTimeUtc) < 0
                        ? _FSI.LastAccessTimeUtc.ToLocalTime() : _FSI.LastWriteTimeUtc.ToLocalTime()
                        : DateTime.Compare(_FSI.CreationTimeUtc, _FSI.LastWriteTimeUtc) < 0
                        ? _FSI.CreationTimeUtc.ToLocalTime() : _FSI.LastWriteTimeUtc.ToLocalTime();
            }
        }

        public override bool Exists
        {
            get { return _FSI.Exists; }
        }

        public override bool IsFile
        {
            get { return (_FSI is DiscFileInfo); }
        }

        public override bool IsFileContainer
        {
            get { return _FSI is DiscDirectoryInfo; }
        }

        public override bool IsItemContainer
        {
            get { return IsFileContainer; }
        }
    }
}
