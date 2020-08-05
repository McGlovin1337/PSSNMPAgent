using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Management;
using Microsoft.Win32;

namespace PSSNMPAgent.SNMPTrapCmdlets
{
    public class BaseSNMPTrap: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Specify Community name to match")]
        public string[] Community { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Specify Destination to match")]
        public string[] Destination { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 3, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        protected virtual void ProcessSNMPTrap(IEnumerable<SNMPTrap> SNMPTraps, IEnumerable<string> ValidDestinationQuery)
        {
            throw new NotImplementedException("ProcessSNMPTrap Method should be overriden");
        }

        protected override void ProcessRecord()
        {
            IEnumerable<SNMPTrap> SNMPTraps;
            IEnumerable<string> DestinationQuery = null;

            if (MyInvocation.BoundParameters.ContainsKey("Destination"))
            {
                WriteVerbose("Validating " + Destination.Count() + " submitted Destination hostnames...");
                IEnumerable<ValidateHostname> validatedHosts = ValidateHostname.ValidateHostnames(Destination);

                IEnumerable<string> ValidHosts = validatedHosts.Where(valid => valid.ValidHostnames != null || valid.ValidHostnames != "").Select(valid => valid.ValidHostnames);
                IEnumerable<string> InvalidHosts = validatedHosts.Where(valid => valid.InvalidHostnames != null).Select(valid => valid.InvalidHostnames);
                WriteVerbose("Yielded " + ValidHosts.Count() + " Valid Hostnames.");
                WriteVerbose("Yielded " + InvalidHosts.Count() + " Invalid Hostnames.");
                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                {
                    foreach (string invalidHost in InvalidHosts)
                        WriteVerbose(invalidHost + " is not a valid hostname!");
                }

                DestinationQuery = ValidHosts;
            }

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Validating " + Computer + " is a valid hostname...");
                bool ValidateComputer = ValidateHostname.ValidateHost(Computer);
                WriteVerbose("Validation result is " + ValidateComputer);

                if (ValidateComputer == false)
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Computer);

                WriteVerbose("Retrieving list of current SNMP Trap Community Names and Destinations from Computer: " + Computer);
                SNMPTraps = RemoteGetSNMPTrap(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retrieving current SNMP Trap Communities and Destinations...");
                SNMPTraps = GetSNMPTraps();
            }

            ProcessSNMPTrap(SNMPTraps, DestinationQuery);
        }

        protected static IEnumerable<SNMPTrap> GetSNMPTraps()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegTrap = Registry.LocalMachine.OpenSubKey(common.RegTraps);

            List<SNMPTrap> traps = new List<SNMPTrap>();
            List<string> destinations = new List<string>();

            foreach (string key in RegTrap.GetSubKeyNames())
            {
                string subkey = common.RegTraps + @"\" + key;
                RegistryKey RegTrapDest = Registry.LocalMachine.OpenSubKey(subkey);
                foreach (string value in RegTrapDest.GetValueNames())
                {
                    destinations.Add((string)RegTrapDest.GetValue(value));
                }
                RegTrapDest.Close();
                traps.Add(new SNMPTrap { Community = key, Destination = destinations.ToArray() });
                destinations.Clear();
            }
            RegTrap.Close();

            return traps;
        }

        protected static IEnumerable<SNMPTrap> RemoteGetSNMPTrap(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            List<SNMPTrap> traps = new List<SNMPTrap>();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumKey");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegTraps;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumKey", mboIn, null);
            string[] subKeyNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            if (subKeyNames == null) return traps;

            foreach (var subKeyName in subKeyNames)
            {
                string subKey = common.RegTraps + @"\" + subKeyName;
                mboIn = mc.GetMethodParameters("EnumValues");
                mboIn["hDefKey"] = (UInt32)2147483650;
                mboIn["sSubKeyName"] = subKey;

                mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
                string[] Values = (string[])mboOut["sNames"];

                mboIn.Dispose();
                mboOut.Dispose();

                if (Values == null)
                {
                    traps.Add(new SNMPTrap { Community = subKeyName, Destination = null });
                }
                else
                {
                    List<string> destinations = new List<string>();
                    foreach (var value in Values)
                    {
                        mboIn = mc.GetMethodParameters("GetStringValue");
                        mboIn["hDefKey"] = (UInt32)2147483650;
                        mboIn["sSubKeyName"] = subKey;
                        mboIn["sValueName"] = value;

                        mboOut = mc.InvokeMethod("GetStringValue", mboIn, null);
                        destinations.Add((string)mboOut["sValue"]);

                        mboIn.Dispose();
                        mboOut.Dispose();
                    }
                    traps.Add(new SNMPTrap { Community = subKeyName, Destination = destinations.ToArray() });
                }
            }

            return traps;
        }

        protected class SNMPTrap
        {
            public string Community { get; set; }
            public string[] Destination { get; set; }
        }

        protected static IEnumerable<SNMPTrap> CompareSNMPTrapResults(IEnumerable<SNMPTrap> Results, IEnumerable<SNMPTrap> Comparisons, bool RemoveCompare = false)
        {
            List<SNMPTrap> confirmed = new List<SNMPTrap>();

            foreach (var result in Results)
            {
                var trapSelect = Comparisons.Where(trap => trap.Community == result.Community);

                if (trapSelect.Count() > 0)
                {
                    foreach (var trap in trapSelect)
                    {
                        if ((result.Destination == null || result.Destination.Count() == 0) && trap.Destination == null)
                        {
                            confirmed.Add(new SNMPTrap { Community = result.Community });
                        }
                        else
                        {
                            var resultDestIntersect = result.Destination.Intersect(trap.Destination);

                            if (resultDestIntersect.Count() == trap.Destination.Count())
                            {
                                confirmed.Add(new SNMPTrap { Community = result.Community, Destination = resultDestIntersect.ToArray() });
                            }
                            else
                            {
                                List<string> confirmedDestinations = new List<string>();
                                foreach (string destination in trap.Destination)
                                {
                                    bool destExist = Array.Exists(result.Destination, dest => dest == destination);

                                    if (destExist == true)
                                        confirmedDestinations.Add(destination);
                                }
                                if (RemoveCompare == false || RemoveCompare == true && confirmedDestinations.Count() > 0)
                                {
                                    confirmed.Add(new SNMPTrap { Community = result.Community, Destination = confirmedDestinations.ToArray() });
                                    confirmedDestinations.Clear();
                                }
                            }
                        }
                    }
                }
            }
            return confirmed;
        }
    }
}