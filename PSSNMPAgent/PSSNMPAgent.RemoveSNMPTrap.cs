﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using PSSNMPAgent.Remote;

namespace RemoveSNMPTrap.cmd
{
    [Cmdlet(VerbsCommon.Remove, nameof(SNMPTrap))]
    [OutputType(typeof(SNMPTrap))]

    public class RemoveSNMPTrap: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Specify Community Names")]
        public string[] Community { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Specify SNMP Trap Destinations to remove from specified Comunity Names")]
        public string[] Destination { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 3, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        private static IEnumerable<SNMPTrap> _SNMPTrap;
        private static IEnumerable<SNMPTrap> _DelTrap;

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Destination"))
            {
                foreach (string Host in Destination)
                {
                    var Match = Regex.Match(Host, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                    if (!Match.Success)
                    {
                        throw new ArgumentException("Specified Destination is not a valid hostname: " + Host);
                    }
                }
            }

            List<SNMPTrap> delTraps = new List<SNMPTrap>();

            if (Community.Count() > 0 && Destination != null)
            {
                foreach (string communityName in Community)
                {
                    foreach (string dest in Destination)
                    {
                        delTraps.Add(new SNMPTrap { Community = communityName, Destination = dest });
                    }
                }
            }

            _DelTrap = delTraps;

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                var Match = Regex.Match(Computer, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                if (!Match.Success)
                {
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Host);
                }
                WriteVerbose("Retrieving current SNMP Trap Communities and Destinations from Computer: " + Computer);
                _SNMPTrap = SNMPRemote.RemoteGetSNMPTrap(Computer, Credential);
            }
            else
            {

                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retrieving current SNMP Trap Communities and Destinations...");
                _SNMPTrap = SNMPAgentCommon.GetSNMPTraps();
            }

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPTrap;
            var delTraps = _DelTrap;

            if (Community.Count() > 0 && Destination == null)
            {
                if (MyInvocation.BoundParameters.ContainsKey("Computer"))
                {
                    WriteVerbose("Removing SNMP Trap Community and all associated Destinations from Computer: " + Computer);
                    SNMPRemote.RemoteDelCommunity(Community, Computer, Credential);
                }
                else
                {
                    WriteVerbose("Removing SNMP Trap Community and all associated Destinations...");
                    DelCommunity(Community);
                }
            }
            else
            {
                var removeTraps = results.Intersect(delTraps, new SNMPTrapComparer());
                if (MyInvocation.BoundParameters.ContainsKey("Computer"))
                {
                    WriteVerbose("Removing specified SNMP Trap Destinations for specified Community Names from Computer: " + Computer);
                    SNMPRemote.RemoteDelTraps(removeTraps, Computer, Credential);
                }
                else
                {
                    WriteVerbose("Removing specified SNMP Trap Destinations for specified Community Names...");
                    DelTraps(removeTraps);
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Retrieving current SNMP Trap Communities and Destinations from Computer: " + Computer);
                _SNMPTrap = SNMPRemote.RemoteGetSNMPTrap(Computer, Credential);
            }
            else
            {
                WriteVerbose("Retrieving current SNMP Trap Communities and Destinations...");
                _SNMPTrap = SNMPAgentCommon.GetSNMPTraps();
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPTrap;
            var delTraps = _DelTrap;

            if (delTraps.Count() == 0 && Community.Count() > 0 && Destination == null)
            {
                results = results.Where(result => Community.Contains(result.Community));
            }
            else
            {
                results = results.Intersect(delTraps, new SNMPTrapComparer());
            }

            if (results.Count() > 0)
            {
                WriteObject("Failed to remove the following SNMP Trap Communities/Destinations...");
                results.ToList().ForEach(WriteObject);
            }
            else
            {
                WriteObject("Successfully removed all specified SNMP Trap Communities/Destinations");
            }            

            base.EndProcessing();
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
                var delTarget = values.Single(value => value.SubKey == trap.Community && value.Value == trap.Destination);
                SubKey.DeleteValue(delTarget.ValueName);
                SubKey.Close();
            }
        }
    }
}