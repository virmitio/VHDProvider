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

namespace VHDProvider{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Text.RegularExpressions;
    using ClrPlus.Powershell.Provider.Base;
    using ClrPlus.Powershell.Provider.Utility;

   

    public class VHDProviderInfo : UniversalProviderInfo {
        //internal static AzureLocation AzureNamespace = new AzureLocation(null, new Path(), null);
        internal static VHDProviderInfo NamespaceProvider;

        /* public CmdletProvider GetProvider() {
            return this.foo();
        } */
        
        internal Collection<PSDriveInfo> AddingDrives = new Collection<PSDriveInfo>();

        public override ILocation GetLocation(string path)
        {
            throw new PSNotImplementedException();
        }

        protected override string Prefix {
            get
            {
                throw new PSNotImplementedException();
                // return AzureDriveInfo.ProviderScheme;
            }
        }

        public VHDProviderInfo(ProviderInfo providerInfo)
            : base(providerInfo) {
        }

    }
}