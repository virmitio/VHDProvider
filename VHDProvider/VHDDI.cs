using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace VHDProvider
{
    class VHDDI : PSDriveInfo
    {


        protected VHDDI(PSDriveInfo driveInfo) : base(driveInfo)
        {
        }

        public VHDDI(string name, ProviderInfo provider, string root, string description, PSCredential credential) : base(name, provider, root, description, credential)
        {
        }


    }
}
