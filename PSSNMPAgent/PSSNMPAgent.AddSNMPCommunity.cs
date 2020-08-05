using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPCommunityCmdlets
{
    [Cmdlet(VerbsCommon.Add, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class PSSNMPAgent: BaseSNMPCommunity
    {
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Add new SNMP Community with the given Access Rights")]
        [ValidateSet("None", "Notify", "ReadOnly", "ReadWrite", "ReadCreate")]
        public string AccessRight { get; set; }

        protected override void ProcessSNMPCommunity(IEnumerable<SNMPCommunity> SNMPCommunities)
        {
            // Prepare
            if (SNMPCommunities.Count() > 0)
            {
                SNMPCommunities = SNMPCommunities.Where(community => Community.Contains(community.Community));
            }

            if (SNMPCommunities.Count() > 0)
            {
                WriteVerbose("Community Names to be added already exist.");

                foreach (var communityName in SNMPCommunities)
                {
                    WriteVerbose("Removing Community Name \"" + communityName.Community + "\" from list of Community Names to be added...");
                    Community = Community.Where(community => community != communityName.Community).ToArray();
                }
            }

            if (Community.Count() == 0)
                throw new Exception("Submitted Community Names List is empty!");

            // Execute
            WriteVerbose("Adding " + Community.Count() + " Communities...");
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                RemoteAddSNMPCommunity(Community, AccessRight, Computer, Credential);

                WriteVerbose("Retrieving list of current SNMP Communities from Computer: " + Computer);
                SNMPCommunities = RemoteGetSNMPCommunity(Computer, Credential);
            }
            else
            {
                AddCommunities(Community, AccessRight);

                WriteVerbose("Retrieving list of current SNMP Communities...");
                SNMPCommunities = GetCommunities();
            }

            // Confirm
            if (SNMPCommunities.Count() > 0)
            {
                SNMPCommunities = SNMPCommunities.Where(community => Community.Contains(community.Community));
            }

            if (SNMPCommunities.Count() == Community.Count())
            {
                WriteObject("Successfully added all SNMP Community Names");
            }
            else if (SNMPCommunities.Count() > 0)
            {
                string[] communityArr = SNMPCommunities.Select(community => community.Community).ToArray();
                foreach (string communityName in Community)
                {
                    bool Match = communityArr.Contains(communityName);
                    if (Match == false)
                    {
                        WriteObject("Failed to add Community Name: " + communityName);
                    }
                }
            }
            else
            {
                throw new Exception("Failed to add specified SNMP Community Names");
            }

            SNMPCommunities.ToList().ForEach(WriteObject);
        }

        private static void AddCommunities(string[] Communities, string Access)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegCommunities = Registry.LocalMachine.CreateSubKey(common.RegCommunities);
            int AccessValue = CommunityAccess.Single(val => val.Access == Access).dWordVal;

            foreach (string Community in Communities)
            {
                RegCommunities.SetValue(Community, AccessValue);
            }
            RegCommunities.Close();
        }

        private static void RemoteAddSNMPCommunity(string[] Communities, string Access, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            int AccessValue = CommunityAccess.Single(val => val.Access == Access).dWordVal;

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("SetDWORDValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            foreach (string Community in Communities)
            {
                mboIn["sValueName"] = Community;
                mboIn["uValue"] = AccessValue;

                mc.InvokeMethod("SetDWORDValue", mboIn, null);
            }

            mboIn.Dispose();
        }
    }
}