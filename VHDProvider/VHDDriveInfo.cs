//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Copyright (c) 2010-2013 Garrett Serack and CoApp Contributors. 
//     Contributors can be discovered using the 'git log' command.
//     All rights reserved.
// </copyright>
// <license>
//     The software is licensed under the Apache 2.0 License (the "License")
//     You may not use the software except in compliance with the License. 
// </license>
//-----------------------------------------------------------------------


using DiscUtils;

namespace VHDProvider {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using ClrPlus.Core.Collections;
    using ClrPlus.Core.Exceptions;
    using ClrPlus.Core.Extensions;
    using ClrPlus.Powershell.Provider.Utility;
    using ClrPlus.Scripting.Languages.PropertySheet;
    using SPath = System.IO.Path;
    using Directory = System.IO.Directory;
    using File = System.IO.File;

    public class VHDDriveInfo : PSDriveInfo {

        //public const string SAS_GUID = "C28BE884-16CF-4401-8B30-18217CF8FF0D";


        internal List<string> RootFiles;
        internal List<int> RootPartitions;
        internal string InternalRootPath;
        internal List<VirtualDisk> HomeDisks;

        internal const string ProviderScheme = "vdisk";
        internal const string ProviderDescription = "Offline Virutual Disk";

        internal Path Path;
        internal string Secret;
//        private CloudStorageAccount _account;
//        private CloudBlobClient _blobStore;
//        private readonly IDictionary<string, CloudBlobContainer> _containerCache = new XDictionary<string, CloudBlobContainer>();

        private Uri _baseUri;
        private string _accountName;

        internal string HostAndPort {
            get {
                throw new PSNotImplementedException();
                return Path.HostAndPort;
            }
        }

        internal string ContainerName {
            get {
                throw new PSNotImplementedException();
                return Path.Container;
            }
        }

        internal string RootPath {
            get {
                throw new PSNotImplementedException();
                return Path.SubPath;
            }
        }

        public VHDDriveInfo(Rule aliasRule, ProviderInfo providerInfo, PSCredential psCredential = null)
            : this(GetDriveInfo(aliasRule, providerInfo, psCredential)) {

            // continues where the GetDriveInfo left off.

            /*
            Path = new Path {
                HostAndPort = aliasRule.HasProperty("key") ? aliasRule["key"].Value : aliasRule.Parameter,
                Container = aliasRule.HasProperty("container") ? aliasRule["container"].Value : "",
                SubPath = aliasRule.HasProperty("root") ? aliasRule["root"].Value.Replace('/', '\\').Replace("\\\\", "\\").Trim('\\') : "",
            };
            Path.Validate();
            Secret = aliasRule.HasProperty("secret") ? aliasRule["secret"].Value : psCredential != null ? psCredential.Password.ToUnsecureString() : null;
             * */
            // Path.Validate();
            // Secret = aliasRule.HasProperty("secret") ? aliasRule["secret"].Value : psCredential != null ? psCredential.Password.ToUnsecureString() : null;
        }

        private static PSDriveInfo GetDriveInfo(Rule aliasRule, ProviderInfo providerInfo, PSCredential psCredential) {
            throw new PSNotImplementedException();
            var name = aliasRule.Parameter;
            var account = aliasRule.HasProperty("key") ? aliasRule["key"].Value : name;
            var container = aliasRule.HasProperty("container") ? aliasRule["container"].Value : "";

            if(psCredential == null || (psCredential.UserName == null && psCredential.Password == null)) {
                psCredential = new PSCredential(account, aliasRule.HasProperty("secret") ? aliasRule["secret"].Value.ToSecureString() : null);
            } 

            if (string.IsNullOrEmpty(container)) {
                return new PSDriveInfo(name, providerInfo, @"{0}:\{1}\".format(ProviderScheme, account), ProviderDescription, psCredential);
            }

            var root = aliasRule.HasProperty("root") ? aliasRule["root"].Value.Replace('/', '\\').Replace("\\\\", "\\").Trim('\\') : "";

            if (string.IsNullOrEmpty(root)) {
                return new PSDriveInfo(name, providerInfo, @"{0}:\{1}\{2}\".format(ProviderScheme, account, container), ProviderDescription, psCredential);
            }

            return new PSDriveInfo(name, providerInfo, @"{0}:\{1}\{2}\{3}\".format(ProviderScheme, account, container, root), ProviderDescription, psCredential);
        }

        public VHDDriveInfo(PSDriveInfo driveInfo)
            : base(driveInfo) {
                RootFiles = new List<string>();
                HomeDisks = new List<VirtualDisk>();
                Init(driveInfo.Provider, driveInfo.Root);
        }

        public VHDDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential)
            : base(name, provider, root, description, credential) {
                RootFiles = new List<string>();
                HomeDisks = new List<VirtualDisk>();
                Init(provider, root);
        }

        /*
        public static string SetRoot(string root, PSCredential credential) {
            if (credential != null && credential.UserName.Contains(" "))
            {
                var sasUsernamePieces = credential.UserName.Split(' ');
                if (sasUsernamePieces.Length != 2)
                {
                    throw new ClrPlusException("Wrong number of SASUsername pieces, should be 2");

                }

                if (!sasUsernamePieces[1].IsWebUri())
                    throw new ClrPlusException("Second part of SASUsername must be a valid Azure Uri");

                var containerUri = new Uri(sasUsernamePieces[1]);

                //TODO Do I actually need to flip the slashes here? I'll do it to be safe for now
                root = @"azure:\\{0}\{1}".format(sasUsernamePieces[0], containerUri.AbsolutePath.Replace('/', '\\'));
                
                SasAccountUri = "https://" + containerUri.Host;
                SasContainer = containerUri.AbsolutePath;




                //it's a SASToken!
            }

            return root;
        }*/

        private void Init(ProviderInfo provider, string root) {

//            var parsedPath = Path.ParseWithContainer(root);
            var parsedPath = Path.ParsePath(root);

            // First, is this a relative path to a file?
            //// Can't do this right now.  Will code it after the path parser is fixed.

            string currentPath;
            int pNum = 0;

            // UNC path?
            if (parsedPath.IsUnc)
                currentPath = SPath.DirectorySeparatorChar + SPath.DirectorySeparatorChar +
                              parsedPath.HostAndPort + SPath.DirectorySeparatorChar + parsedPath.Container;
            else
                // File on local filesystem?
                currentPath = parsedPath.Drive + SPath.DirectorySeparatorChar;

            while (pNum < parsedPath.Parts.Length)
            {
                while (Directory.Exists(currentPath) && pNum < parsedPath.Parts.Length)
                    currentPath += SPath.DirectorySeparatorChar + parsedPath.Parts[pNum++];
                // at this point we must be at either a path that isn't a directory or then end of the path specified.
                if (!File.Exists(currentPath))
                    throw new System.IO.FileNotFoundException(currentPath);

                try
                {
                    HomeDisks.Add(VirtualDisk.OpenDisk(currentPath, System.IO.FileAccess.ReadWrite));
                }
                catch
                {
                    HomeDisks.Add(VirtualDisk.OpenDisk(currentPath, System.IO.FileAccess.Read));
                }

                if (HomeDisks.IsNullOrEmpty())
                    throw new ClrPlusException("Unknown virtual disk format: {0}".format(currentPath));

                RootFiles.Add(currentPath);

                // if (!HomeDisks.Next(?).IsPartitioned)
                if (!HomeDisks.First().IsPartitioned)
                    throw new ClrPlusException("Virtual disk contains no partitions: {0}".format(currentPath));

                if (pNum < parsedPath.Parts.Length)
                {
                    if (!int.TryParse(parsedPath.Parts[pNum++], out RootPartition))
                        throw new ClrPlusException(
                            "{0} is not a valid partition index".format(parsedPath.Parts[pNum - 1]));

                    // parition specified...


                }
                else
                {
                    // No partition specified.

                }
            }


            if (parsedPath.Name.IndexOf('.') < 0)
                throw new ClrPlusException("Invalid path to virtual disk");
            if (!(new [] {".vhd", ".vhdx", ".vmdk"}.ContainsIgnoreCase(parsedPath.Name.Substring(parsedPath.Name.LastIndexOf('.')))))
                throw new ClrPlusException("Invalid path to virtual disk");

            Path = parsedPath;
            
            if (string.IsNullOrEmpty(parsedPath.HostAndPort) || string.IsNullOrEmpty(parsedPath.Scheme)) {
                Path = parsedPath;
                return;
            }

            var pi = provider as VHDProviderInfo;
            if (pi == null) {
                throw new ClrPlusException("Invalid ProviderInfo");
            }

            // var alldrives = (pi.AddingDrives.Union(pi.Drives)).Select(each => each as VHDDriveInfo).ToArray();

            if (parsedPath.Scheme == ProviderScheme) {
                // it's being passed a full url to a virtual disk
                Path = parsedPath;
            }
            
        }

        internal string ActualHostAndPort
        {
            get
            {
                throw new PSNotImplementedException();
                return _baseUri == null ? "" : (((_baseUri.Scheme == "https" && _baseUri.Port == 443) || (_baseUri.Scheme == "http" && _baseUri.Port == 80) || _baseUri.Port == 0) ? _baseUri.Host : _baseUri.Host + ":" + _baseUri.Port);
            }
        }
    }
    /*
    internal class RelativeBlobDirectoryUri
    {
        internal string Container { get; private set; }
        internal IEnumerable<string> VirtualDirectories { get; private set; }

        internal RelativeBlobDirectoryUri(string relativeBlobDirectoryUri)
        {
            var splitString = relativeBlobDirectoryUri.Split('/');
            //if (splitString.Length ==0) bad!!!
            if (splitString.Length >= 1)
            {
                Container = splitString[0];

            }
            if (splitString.Length >= 2)
            {
                VirtualDirectories = splitString.Skip(1).ToList();
            }

        }
    }*/
 
}