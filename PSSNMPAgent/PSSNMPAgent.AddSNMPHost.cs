using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AddSNMPHost.cmd
{
    [Cmdlet(VerbsCommon.Add, nameof(SNMPHost))]
    [OutputType(typeof(SNMPHost))]

    public class AddSNMPHost: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Add a new SNMP Permitted Manager")]
        public string[] Hosts { get; set; }

        private static IEnumerable<SNMPHost> _SNMPHosts;

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Hosts"))
            {
                foreach (string Host in Hosts)
                {
                    var Match = Regex.Match(Host, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                    if (!Match.Success)
                    {
                        throw new ArgumentException("Specified Host is not a valid hostname: " + Host);
                    }
                }
            }
            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            _SNMPHosts = SNMPAgentCommon.GetSNMPHosts();            

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPHosts;

            string[] LowerHost = Array.ConvertAll(Hosts, host => host.ToLower());

            if (results.Count() > 0)
            {
                results = results.Where(result => LowerHost.Contains(result.Host.ToLower()));
            }

            if (results.Count() > 0)
            {
                throw new Exception("SNMP Host already exists");
            }

            WriteVerbose("Adding " + Hosts.Count() + " hosts...");
            AddSNMPHosts(Hosts);

            _SNMPHosts = SNMPAgentCommon.GetSNMPHosts();

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPHosts;
            string[] LowerHost = Array.ConvertAll(Hosts, host => host.ToLower());

            results = results.Where(result => LowerHost.Contains(result.Host.ToLower()));

            results.ToList().ForEach(WriteObject);

            base.EndProcessing();
        }

        private static void AddSNMPHosts(string[] Hosts)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegHosts = Registry.LocalMachine.OpenSubKey(common.RegHosts);
            List<int> valueNames = new List<int>();

            foreach (string valueName in RegHosts.GetValueNames())
            {
                if (valueName != "(Default)")
                {
                    valueNames.Add(Convert.ToInt32(valueName));
                }
            }
            RegHosts.Close();

            RegHosts = Registry.LocalMachine.CreateSubKey(common.RegHosts);
            int val = 1;
            String Value;
            bool valExist = true;
            foreach (var Host in Hosts)
            {
                while (valExist == true)
                {
                    valExist = valueNames.Contains(val);
                    if (valExist == true) { val++; }
                }
                Value = Convert.ToString(val);
                RegHosts.SetValue(Value, Host);
                valueNames.Add(val);
                valExist = true;
                val = 1;
            }
            RegHosts.Close();
        }
    }
}