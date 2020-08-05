using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPTrapCmdlets
{
    [Cmdlet(VerbsCommon.Remove, nameof(SNMPTrap))]
    [OutputType(typeof(SNMPTrap))]

    public class RemoveSNMPTrap: BaseSNMPTrap
    {
        protected override void ProcessSNMPTrap(IEnumerable<SNMPTrap> SNMPTraps, IEnumerable<string> ValidDestinationQuery)
        {
            // Prepare
            IEnumerable<SNMPTrap> DeleteTraps = null;

            if (Community.Count() > 0 && ValidDestinationQuery != null)
            {
                List<SNMPTrap> delTraps = new List<SNMPTrap>();

                foreach (string communityName in Community)
                {
                    delTraps.Add(new SNMPTrap { Community = communityName, Destination = ValidDestinationQuery.ToArray() });
                }

                DeleteTraps = delTraps;
            }

            // Execute
            if (Community.Count() > 0 && ValidDestinationQuery == null)
            {
                List<string> resultCommunities = new List<string>();
                foreach (var trap in SNMPTraps)
                {
                    resultCommunities.Add(trap.Community);
                }

                string[] preIntersectCommunity = Community;
                Community = Community.Intersect(resultCommunities).ToArray();

                preIntersectCommunity = preIntersectCommunity.Except(Community).ToArray();

                if (preIntersectCommunity.Count() > 0 && MyInvocation.BoundParameters.ContainsKey("Verbose"))
                    foreach (var community in preIntersectCommunity)
                        WriteVerbose("The SNMP Trap Community " + community + " does not exist.");

                if (Community.Count() > 0)
                {
                    if (MyInvocation.BoundParameters.ContainsKey("Computer"))
                    {
                        WriteVerbose("Removing SNMP Trap Community and all associated Destinations from Computer: " + Computer);
                        RemoteDelCommunity(Community, Computer, Credential);
                    }
                    else
                    {
                        WriteVerbose("Removing SNMP Trap Community and all associated Destinations...");
                        DelCommunity(Community);
                    }
                }
                else
                {
                    throw new Exception("No SNMP Trap Community Names to remove");
                }
            }
            else
            {
                WriteVerbose("Checking requested SNMP Trap Destinations to be removed exist...");
                IEnumerable<SNMPTrap> removeTraps = CompareSNMPTrapResults(SNMPTraps, DeleteTraps, true);

                if (removeTraps.Count() == 0)
                    throw new Exception("No SNMP Trap Destinations to remove");

                if (MyInvocation.BoundParameters.ContainsKey("Computer"))
                {
                    WriteVerbose("Removing specified SNMP Trap Destinations for specified Community Names from Computer: " + Computer);
                    RemoteDelTraps(removeTraps, Computer, Credential);
                }
                else
                {
                    WriteVerbose("Removing specified SNMP Trap Destinations for specified Community Names...");
                    DelTraps(removeTraps);
                }
            }

            //Confirm
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Retrieving current SNMP Trap Communities and Destinations from Computer: " + Computer);
                SNMPTraps = RemoteGetSNMPTrap(Computer, Credential);
            }
            else
            {
                WriteVerbose("Retrieving current SNMP Trap Communities and Destinations...");
                SNMPTraps = GetSNMPTraps();
            }

            if (DeleteTraps == null && Community.Count() > 0 && ValidDestinationQuery == null)
            {
                SNMPTraps = SNMPTraps.Where(trap => Community.Contains(trap.Community));
            }
            else
            {
                SNMPTraps = CompareSNMPTrapResults(SNMPTraps, DeleteTraps);
            }

            if (SNMPTraps.Count() > 0)
            {
                if (ValidDestinationQuery != null)
                {
                    SNMPTraps = SNMPTraps.Where(trap => trap.Destination.Count() > 0);
                }

                if (SNMPTraps.Count() > 0)
                {
                    WriteObject("Failed to remove the following SNMP Trap Communities/Destinations...");
                    SNMPTraps.ToList().ForEach(WriteObject);
                }
                else
                {
                    WriteObject("Successfully removed all specified SNMP Trap Destinations from specified Community Names");
                }
            }
            else
            {
                WriteObject("Successfully removed all specified SNMP Trap Communities");
            }
        }

        private static void DelCommunity(string[] Community)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            foreach (string community in Community)
            {
                RegistryKey SubKey = Registry.LocalMachine.CreateSubKey(common.RegTraps);
                SubKey.DeleteSubKey(community);
            }
        }

        private static void DelTraps(IEnumerable<SNMPTrap> delTraps)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            string RegTrapRoot = common.RegTraps;

            string[] subKeys = delTraps.Select(x => x.Community).Distinct().ToArray();
            List<RegTrapValueMap> values = new List<RegTrapValueMap>();
            foreach (var key in subKeys)
            {
                RegistryKey SubKey = Registry.LocalMachine.OpenSubKey(RegTrapRoot + @"\" + key);
                foreach (string valueName in SubKey.GetValueNames())
                {
                    string value = (string)SubKey.GetValue(valueName);
                    values.Add(new RegTrapValueMap { SubKey = key, ValueName = valueName, Value = value });
                }
                SubKey.Close();
            }

            foreach (var trap in delTraps)
            {
                string keyTarget = RegTrapRoot + @"\" + trap.Community;
                RegistryKey SubKey = Registry.LocalMachine.CreateSubKey(keyTarget);
                foreach (string destination in trap.Destination)
                {
                    var delTarget = values.Where(value => value.SubKey == trap.Community && value.Value == destination);
                    foreach (var target in delTarget)
                    {
                        SubKey.DeleteValue(target.ValueName);                        
                    }
                }
                SubKey.Close();
            }
        }

        private static void RemoteDelCommunity(string[] Community, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("DeleteKey");
            mboIn["hDefKey"] = (UInt32)2147483650;

            foreach (string community in Community)
            {
                mboIn["sSubKeyName"] = common.RegTraps + @"\" + community;

                mc.InvokeMethod("DeleteKey", mboIn, null);
            }
            mboIn.Dispose();
        }

        private static void RemoteDelTraps(IEnumerable<SNMPTrap> delTraps, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            string[] subKeys = delTraps.Select(x => x.Community).Distinct().ToArray();
            List<RegTrapValueMap> values = new List<RegTrapValueMap>();

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;

            foreach (var key in subKeys)
            {
                mboIn["sSubKeyName"] = common.RegTraps + @"\" + key;

                ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
                string[] valueNames = (string[])mboOut["sNames"];
                mboOut.Dispose();

                ManagementBaseObject mboIn2 = mc.GetMethodParameters("GetStringValue");
                mboIn2["hDefKey"] = (UInt32)2147483650;
                mboIn2["sSubKeyName"] = common.RegTraps + @"\" + key;

                foreach (string valueName in valueNames)
                {
                    mboIn2["sValueName"] = valueName;

                    ManagementBaseObject mboOut2 = mc.InvokeMethod("GetStringValue", mboIn2, null);
                    string value = (string)mboOut2["sValue"];
                    mboOut2.Dispose();

                    values.Add(new RegTrapValueMap { SubKey = key, ValueName = valueName, Value = value });
                }
                mboIn2.Dispose();
            }
            mboIn.Dispose();

            mboIn = mc.GetMethodParameters("DeleteValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            foreach (var trap in delTraps)
            {
                mboIn["sSubKeyName"] = common.RegTraps + @"\" + trap.Community;
                foreach (string destination in trap.Destination)
                {
                    var delTarget = values.Single(value => value.SubKey == trap.Community && value.Value == destination);
                    mboIn["sValueName"] = delTarget.ValueName;
                    mc.InvokeMethod("DeleteValue", mboIn, null);
                }
            }
            mboIn.Dispose();
        }

        private class RegTrapValueMap
        {
            public string SubKey { get; set; }
            public string ValueName { get; set; }
            public string Value { get; set; }
        }
    }
}