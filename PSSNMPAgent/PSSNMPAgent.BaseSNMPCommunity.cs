using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;

namespace PSSNMPAgent.SNMPCommunityCmdlets
{
    public class BaseSNMPCommunity: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Specify SNMP Community Names")]
        public string[] Community { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 3, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        protected virtual void ProcessSNMPCommunity(IEnumerable<SNMPCommunity> SNMPCommunities)
        {
            throw new NotImplementedException("ProcessSNMPCommunity Method should be overriden");
        }

        protected override void ProcessRecord()
        {
            IEnumerable<SNMPCommunity> SNMPCommunities;

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Validating " + Computer + " is a valid hostname...");
                bool ValidateComputer = ValidateHostname.ValidateHost(Computer);
                WriteVerbose("Validation result is " + ValidateComputer);

                if (ValidateComputer == false)
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Computer);

                WriteVerbose("Retrieving list of current SNMP Community Names from Computer: " + Computer);
                SNMPCommunities = RemoteGetSNMPCommunity(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retrieving list of current SNMP Communities...");
                SNMPCommunities = GetCommunities();
            }

            ProcessSNMPCommunity(SNMPCommunities);
        }

        protected static IEnumerable<SNMPCommunity> GetCommunities()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegCommunities = Registry.LocalMachine.OpenSubKey(common.RegCommunities);

            List<SNMPCommunity> communities = new List<SNMPCommunity>();

            foreach (string Community in RegCommunities.GetValueNames())
            {
                int accessValue = (int)RegCommunities.GetValue(Community);
                var accessType = CommunityAccess.Single(a => a.dWordVal == accessValue);
                string access = accessType.Access;
                communities.Add(new SNMPCommunity { Community = Community, AccessRight = access });
            }
            RegCommunities.Close();

            return communities;
        }

        protected static IEnumerable<SNMPCommunity> RemoteGetSNMPCommunity(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] ValueNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            List<SNMPCommunity> communities = new List<SNMPCommunity>();

            if (ValueNames == null) return communities;

            foreach (var value in ValueNames)
            {
                mboIn = mc.GetMethodParameters("GetDWORDValue");
                mboIn["hDefKey"] = (UInt32)2147483650;
                mboIn["sSubKeyName"] = common.RegCommunities;
                mboIn["sValueName"] = value;

                mboOut = mc.InvokeMethod("GetDWORDValue", mboIn, null);
                var accessType = CommunityAccess.Single(a => a.dWordVal == (UInt32)mboOut["uValue"]);
                string access = accessType.Access;
                communities.Add(new SNMPCommunity { Community = value, AccessRight = access });

                mboIn.Dispose();
                mboOut.Dispose();
            }

            return communities;
        }

        protected class communityAccess
        {
            public string Access;
            public int dWordVal;
        }

        protected class SNMPCommunity
        {
            public string Community { get; set; }
            public string AccessRight { get; set; }
        }

        private static List<communityAccess> _communityAccess = new List<communityAccess>
        {
            new communityAccess() { Access = "None", dWordVal = 1 },
            new communityAccess() { Access = "Notify", dWordVal = 2},
            new communityAccess() { Access = "ReadOnly", dWordVal = 4},
            new communityAccess() { Access = "ReadWrite", dWordVal = 8},
            new communityAccess() { Access = "ReadCreate", dWordVal = 16}
        };

        protected static ReadOnlyCollection<communityAccess> CommunityAccess
        {
            get { return new ReadOnlyCollection<communityAccess>(_communityAccess); }
        }
    }
}