using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPCommunityCmdlets
{
    [Cmdlet(VerbsCommon.Remove, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class RemoveSNMPCommunity: BaseSNMPCommunity
    {
        protected override void ProcessSNMPCommunity(IEnumerable<SNMPCommunity> SNMPCommunities)
        {
            // Prepare
            if (SNMPCommunities.Count() == 0)
            {
                throw new Exception("There are no SNMP Community Names to remove");
            }

            SNMPCommunities = SNMPCommunities.Where(community => Community.Contains(community.Community));

            if (SNMPCommunities.Count() == 0)
            {
                throw new Exception("None of the specified community names were found");
            }

            // Execute
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Removing specified SNMP Community Names from Computer: " + Computer);
                RemoteRemoveSNMPCommunities(SNMPCommunities, Computer, Credential);

                WriteVerbose("Retrieving list of current SNMP Communities from Computer: " + Computer);
                SNMPCommunities = RemoteGetSNMPCommunity(Computer, Credential);
            }
            else
            {
                WriteVerbose("Removing specified SNMP Community Names...");
                RemoveSNMPCommunities(SNMPCommunities);

                WriteVerbose("Retrieving list of current SNMP Communities...");
                SNMPCommunities = GetCommunities();
            }

            // Confirm
            if (SNMPCommunities.Count() > 0)
            {
                SNMPCommunities = SNMPCommunities.Where(community => Community.Contains(community.Community));
            }

            if (SNMPCommunities.Count() == 0)
            {
                WriteObject("Successfully removed specified SNMP Community names");
            }
            else
            {
                foreach (var community in SNMPCommunities)
                {
                    WriteVerbose("Failed to remove Community Name: " + community.Community);
                }
                throw new Exception("Failed to remove all specified SNMP Community names");
            }
        }

        private static void RemoveSNMPCommunities(IEnumerable<SNMPCommunity> Communities)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegCommunities = Registry.LocalMachine.CreateSubKey(common.RegCommunities);

            foreach (var value in Communities)
            {
                if (value.Community != "(Default)")
                {
                    RegCommunities.DeleteValue(value.Community);
                }
            }
            RegCommunities.Close();
        }

        private static void RemoteRemoveSNMPCommunities(IEnumerable<SNMPCommunity> Communities, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("DeleteValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            foreach (var value in Communities)
            {
                if (value.Community != "(Default)")
                {
                    mboIn["sValueName"] = value.Community;

                    mc.InvokeMethod("DeleteValue", mboIn, null);
                }
            }
            mboIn.Dispose();
        }
    }
}