using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace GetSNMPHost.cmd
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPHost))]
    [OutputType(typeof(SNMPHost))]
    public class GetSNMPHost: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List matching Hosts")]
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

            _SNMPHosts = GetSNMPHosts();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPHosts;

            if (results.Count() > 0)
            {
                if (MyInvocation.BoundParameters.ContainsKey("Hosts"))
                {
                    results = results.Where(host => Regex.IsMatch(host.Host, string.Format("^(?i:{0})", string.Join("|", Hosts))));
                }
            }
            else
            {
                WriteVerbose("No hosts configured, SNMP allowed from Any host!");
            }

            results.ToList().ForEach(WriteObject);

            base.ProcessRecord();
        }

        private static IEnumerable<SNMPHost> GetSNMPHosts()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegHosts = Registry.LocalMachine.OpenSubKey(common.RegHosts);

            List<SNMPHost> hosts = new List<SNMPHost>();

            foreach (string value in RegHosts.GetValueNames())
            {
                string host = (string)RegHosts.GetValue(value);
                hosts.Add(new SNMPHost { Host = host });
            }
            RegHosts.Close();

            return hosts;
        }
    }
}