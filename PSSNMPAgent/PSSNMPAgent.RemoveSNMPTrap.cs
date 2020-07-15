using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Text.RegularExpressions;
using Microsoft.Win32;

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

        [Parameter(Position = 2, HelpMessage = "Delete the specified Community Name including all Trap Destinations")]
        public SwitchParameter DeleteCommunity { get; set; }

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

            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            List<SNMPTrap> delTraps = new List<SNMPTrap>();

            if (!MyInvocation.BoundParameters.ContainsKey("DeleteCommunity"))
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
            _SNMPTrap = SNMPAgentCommon.GetSNMPTraps();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPTrap;
            var delTraps = _DelTrap;

            WriteVerbose("delTraps Count = " + delTraps.Count());

            if (delTraps.Count() == 0 && MyInvocation.BoundParameters.ContainsKey("DeleteCommunity"))
            {
                DelCommunity(Community);
            }
            else
            {
                var removeTraps = results.Intersect(delTraps, new SNMPTrapComparer());
                DelTraps(removeTraps);
            }

            _SNMPTrap = SNMPAgentCommon.GetSNMPTraps();

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPTrap;
            var delTraps = _DelTrap;

            if (delTraps.Count() == 0 && MyInvocation.BoundParameters.ContainsKey("DeleteCommunity"))
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
                WriteObject("Successfully removed all SNMP Trap Communities/Destinations");
            }            

            base.EndProcessing();
        }

        private static void DelCommunity(string[] Community)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            foreach (string community in Community)
            {
                string SubKey = common.RegTraps + @"\" + community;
                Registry.LocalMachine.DeleteSubKey(SubKey);
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

        private class RegTrapValueMap
        {
            public string SubKey { get; set; }
            public string ValueName { get; set; }
            public string Value { get; set; }
        }
    }
}