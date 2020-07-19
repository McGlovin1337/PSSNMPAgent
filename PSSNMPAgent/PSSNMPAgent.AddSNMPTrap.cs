using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AddSNMPTrap.cmd
{
    [Cmdlet(VerbsCommon.Add, nameof(SNMPTrap))]
    [OutputType(typeof(SNMPTrap))]

    public class AddSNMPTrap: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Add the SNMP Trap Community names")]
        public string[] Community { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Add Trap Destination hosts to all specified Community Names")]
        public string[] Destination { get; set; }

        private static IEnumerable<SNMPTrap> _SNMPTrap;
        private static IEnumerable<SNMPTrap> _newTraps;
        
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

            List<SNMPTrap> newTraps = new List<SNMPTrap>();

            foreach (var communityName in Community)
            {
                foreach (var dest in Destination)
                {
                    newTraps.Add(new SNMPTrap { Community = communityName, Destination = dest });
                }
            }

            _newTraps = newTraps;
            WriteVerbose("Retrieving current SNMP Trap Communities and Destinations...");
            _SNMPTrap = SNMPAgentCommon.GetSNMPTraps();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPTrap.ToList();
            var newTraps = _newTraps.ToList();

            newTraps.RemoveAll(x => results.Exists(y => y.Community == x.Community && y.Destination == x.Destination));

            if (newTraps.Count() > 0)
            {
                WriteVerbose("Adding SNMP Traps...");
                AddTraps(newTraps);
            }
            else
            {
                throw new Exception("Community Name and Destinations already Exist");
            }

            _newTraps = newTraps;
            _SNMPTrap = SNMPAgentCommon.GetSNMPTraps();

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPTrap;
            var newTraps = _newTraps;

            var confirmed = results.Intersect(newTraps, new SNMPTrapComparer());
            
            confirmed.ToList().ForEach(WriteObject);

            base.EndProcessing();
        }

        private static void AddTraps(IEnumerable<SNMPTrap> newTraps)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegTrapRoot = Registry.LocalMachine.OpenSubKey(common.RegTraps);
            
            IEnumerable<string> TrapKeys = RegTrapRoot.GetSubKeyNames().ToList();
            RegTrapRoot.Close();

            IEnumerable<string> Communities = newTraps.Select(x => x.Community).Distinct();
            IEnumerable<string> Destinations = newTraps.Select(y => y.Destination).Distinct();
            TrapKeys = TrapKeys.Where(key => Communities.Contains(key));

            foreach (string Community in Communities)
            {
                int valueStart = 1;
                bool KeyExist = TrapKeys.Contains(Community);                
                if (KeyExist == true)
                {
                    RegistryKey TrapSubKey = Registry.LocalMachine.OpenSubKey(common.RegTraps + @"\" + Community);
                    List<string> values = TrapSubKey.GetValueNames().ToList();
                    values.RemoveAll(x => x == "(Default)");
                    IEnumerable<int> Values = values.Select(x => int.Parse(x)).ToList();
                    valueStart = Values.Max();
                    valueStart++;
                    TrapSubKey.Close();
                }
                RegistryKey trapSubKey = Registry.LocalMachine.CreateSubKey(common.RegTraps + @"\" + Community);
                foreach (string Destination in Destinations)
                {
                    trapSubKey.SetValue(valueStart.ToString(), Destination);
                    valueStart++;
                }
            }           
        }
    }
}