using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Text.RegularExpressions;

namespace PSSNMPAgent.SNMPHostCmdlets
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPHost))]
    [OutputType(typeof(SNMPHost))]
    public class GetSNMPHost: BaseSNMPHost
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List matching Hosts")]
        [Alias("Hosts", "Host", "Manager", "PermittedManager")]
        public string[] PermittedHost { get; set; }

        private static IEnumerable<string> _validHostsQuery;
        protected override void ProcessRecord()
        {
            if (MyInvocation.BoundParameters.ContainsKey("PermittedHost"))
            {
                WriteVerbose("Validating " + PermittedHost.Count() + " submitted PermittedHost hostnames...");
                IEnumerable<ValidateHostname> validatedHosts = ValidateHostname.ValidateHostnames(PermittedHost);

                IEnumerable<string> ValidHosts = validatedHosts.Where(valid => valid.ValidHostnames != null).Select(valid => valid.ValidHostnames);
                IEnumerable<string> InvalidHosts = validatedHosts.Where(valid => valid.InvalidHostnames != null).Select(valid => valid.InvalidHostnames);
                WriteVerbose("Yielded " + ValidHosts.Count() + " Valid Hostnames.");
                WriteVerbose("Yielded " + InvalidHosts.Count() + " Invalid Hostnames.");
                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                {
                    foreach (string invalidHost in InvalidHosts)
                        WriteVerbose(invalidHost + " is not a valid hostname!");
                }

                _validHostsQuery = ValidHosts;
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            _validHostsQuery = null;
        }

        protected override void ProcessSNMPHost(IEnumerable<SNMPHost> SNMPHosts)
        {            
            IEnumerable<string> validHostsQuery = _validHostsQuery;

            if (SNMPHosts.Count() == 0)
                WriteVerbose("No hosts configured, SNMP allowed from Any host!");

            if (validHostsQuery != null)
            {
                SNMPHosts = SNMPHosts.Where(host => Regex.IsMatch(host.PermittedHost, string.Format("^(?i:{0})", string.Join("|", validHostsQuery))));
                if (SNMPHosts.Count() == 0)
                    WriteVerbose("No PermittedHosts matched query");
            }

            if (PermittedHost != null && validHostsQuery == null)
            {
                WriteVerbose("No PermittedHosts matched query");
                SNMPHosts = new List<SNMPHost>();
            }

            SNMPHosts.ToList().ForEach(WriteObject);
        }
    }
}