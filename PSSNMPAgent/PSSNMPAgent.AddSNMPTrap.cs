using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPTrapCmdlets
{
    [Cmdlet(VerbsCommon.Add, nameof(SNMPTrap))]
    [OutputType(typeof(SNMPTrap))]

    public class AddSNMPTrap: BaseSNMPTrap
    {
        protected override void ProcessSNMPTrap(IEnumerable<SNMPTrap> SNMPTraps, IEnumerable<string> ValidDestinationQuery)
        {
            // Prepare
            List<SNMPTrap> newTraps = new List<SNMPTrap>();

            foreach (var communityName in Community)
            {
                if (ValidDestinationQuery == null)
                {
                    newTraps.Add(new SNMPTrap { Community = communityName });
                }
                else
                {
                    newTraps.Add(new SNMPTrap { Community = communityName, Destination = ValidDestinationQuery.ToArray() });
                }
            }

            List<SNMPTrap> filteredNewTraps = FilterNewTraps(SNMPTraps, newTraps).ToList();

            // Execute
            if (filteredNewTraps.Count() > 0)
            {
                WriteVerbose("Adding SNMP Traps...");
                if (MyInvocation.BoundParameters.ContainsKey("Computer"))
                {
                    RemoteAddSNMPTrap(filteredNewTraps, Computer, Credential);
                    SNMPTraps = RemoteGetSNMPTrap(Computer, Credential);
                }
                else
                {
                    AddTraps(filteredNewTraps);
                    SNMPTraps = GetSNMPTraps();
                }
            }
            else
            {
                throw new Exception("Community Name and Destinations already Exist");
            }

            // Confirm
            IEnumerable<SNMPTrap> confirmed = CompareSNMPTrapResults(SNMPTraps, filteredNewTraps);

            confirmed.ToList().ForEach(WriteObject);
        }

        private static void AddTraps(IEnumerable<SNMPTrap> newTraps)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegTrapRoot = Registry.LocalMachine.OpenSubKey(common.RegTraps);
            
            IEnumerable<string> TrapKeys = RegTrapRoot.GetSubKeyNames().ToList();
            RegTrapRoot.Close();

            foreach (var trap in newTraps)
            {
                int valueStart = 1;
                bool KeyExist = TrapKeys.Contains(trap.Community);
                if (KeyExist == true)
                {
                    RegistryKey TrapSubKey = Registry.LocalMachine.OpenSubKey(common.RegTraps + @"\" + trap.Community);
                    List<string> values = TrapSubKey.GetValueNames().ToList();
                    values.RemoveAll(x => x == "(Default)");
                    if (values.Count() > 0)
                    {
                        IEnumerable<int> Values = values.Select(x => int.Parse(x)).ToList();
                        valueStart = Values.Max() + 1;
                    }
                    TrapSubKey.Close();
                }
                RegistryKey trapSubKey = Registry.LocalMachine.CreateSubKey(common.RegTraps + @"\" + trap.Community);

                if (trap.Destination != null)
                {
                    foreach (string destination in trap.Destination)
                    {
                        trapSubKey.SetValue(valueStart.ToString(), destination);
                        valueStart++;
                    }
                }

                trapSubKey.Close();
            }       
        }

        private static void RemoteAddSNMPTrap(IEnumerable<SNMPTrap> newTraps, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumKey");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegTraps;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumKey", mboIn, null);
            IEnumerable<string> TrapKeys = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            ManagementBaseObject mboIn2 = mc.GetMethodParameters("EnumValues");
            mboIn2["hDefKey"] = (UInt32)2147483650;

            ManagementBaseObject mboIn3 = mc.GetMethodParameters("CreateKey");
            mboIn3["hDefKey"] = (UInt32)2147483650;

            ManagementBaseObject mboIn4 = mc.GetMethodParameters("SetStringValue");
            mboIn4["hDefKey"] = (UInt32)2147483650;

            foreach (var trap in newTraps)
            {
                mboIn2["sSubKeyName"] = common.RegTraps + @"\" + trap.Community;
                mboIn3["sSubKeyName"] = common.RegTraps + @"\" + trap.Community;
                mboIn4["sSubKeyName"] = common.RegTraps + @"\" + trap.Community;
                int valueStart = 1;
                bool KeyExist = (TrapKeys != null) ? TrapKeys.Contains(trap.Community) : false;
                if (KeyExist == true)
                {
                    ManagementBaseObject mboOut2 = mc.InvokeMethod("EnumValues", mboIn2, null);
                    IEnumerable<string> sNames = (string[])mboOut2["sNames"];
                    List<string> values = sNames.ToList();
                    values.RemoveAll(x => x == "(Default)");
                    IEnumerable<int> Values = values.Select(x => int.Parse(x)).ToList();
                    valueStart = Values.Max() + 1;
                    mboOut2.Dispose();
                }
                else
                {
                    mc.InvokeMethod("CreateKey", mboIn3, null);
                }

                if (trap.Destination != null)
                {
                    foreach (string destination in trap.Destination)
                    {
                        mboIn4["sValueName"] = valueStart.ToString();
                        mboIn4["sValue"] = destination;

                        mc.InvokeMethod("SetStringValue", mboIn4, null);
                        valueStart++;
                    }
                }
            }

            mboIn2.Dispose();
            mboIn3.Dispose();
            mboIn4.Dispose();
        }

        private static IEnumerable<SNMPTrap> FilterNewTraps(IEnumerable<SNMPTrap> Results, IEnumerable<SNMPTrap> NewTraps)
        {
            List<SNMPTrap> filteredNewTraps = new List<SNMPTrap>();

            Results = Results.Intersect(NewTraps, new SNMPTrapCommunityComparer());

            var newCommunities = NewTraps.Where(trap => !Results.Any(result => trap.Community == result.Community));

            if (newCommunities.Count() > 0)
            {
                filteredNewTraps.AddRange(newCommunities);
            }

            foreach (var result in Results)
            {
                var trapSelect = NewTraps.Where(trap => trap.Community == result.Community);
               
                foreach (var trap in trapSelect)
                {
                    string[] filteredNewDestinations = trap.Destination.Except(result.Destination).ToArray();
                    filteredNewTraps.Add(new SNMPTrap { Community = trap.Community, Destination = filteredNewDestinations });
                }
            }

            return filteredNewTraps;
        }

        private class SNMPTrapCommunityComparer : EqualityComparer<SNMPTrap>
        {
            public override bool Equals(SNMPTrap trap1, SNMPTrap trap2)
            {
                if (trap1.Community == trap2.Community)
                {
                    return true;
                }
                else return false;
            }
            public override int GetHashCode(SNMPTrap obj)
            {
                return obj.Community.GetHashCode();
            }
        }
    }
}