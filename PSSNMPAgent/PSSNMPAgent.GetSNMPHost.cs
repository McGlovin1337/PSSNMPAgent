using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using PSSNMPAgent.Remote;

namespace GetSNMPHost.cmd
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPHost))]
    [OutputType(typeof(SNMPHost))]
    public class GetSNMPHost : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List matching Hosts")]
        [Alias("Hosts", "Host", "Manager", "PermittedManager")]
        public string[] PermittedHost { get; set; }

        [Parameter(Position = 1, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }        

        private static IEnumerable<SNMPHost> _SNMPHosts;

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey("PermittedHost"))
            {
                foreach (string Host in PermittedHost)
                {
                    var Match = Regex.Match(Host, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                    if (!Match.Success)
                    {
                        throw new ArgumentException("Specified Computer is not a valid hostname: " + Host);
                    }
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                var Match = Regex.Match(Computer, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                if (!Match.Success)
                {
                    throw new ArgumentException("Specified Host is not a valid hostname: " + Host);
                }

                WriteVerbose("Retrieving list of current SNMP Permitted Hosts from Computer: " + Computer);
                _SNMPHosts = SNMPRemote.RemoteGetSNMPHosts(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retrieving list of current SNMP Permitted Hosts...");
                _SNMPHosts = SNMPAgentCommon.GetSNMPHosts();
            }

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPHosts;

            if (results.Count() > 0)
            {
                if (MyInvocation.BoundParameters.ContainsKey("PermittedHost"))
                {
                    results = results.Where(host => Regex.IsMatch(host.PermittedHost, string.Format("^(?i:{0})", string.Join("|", PermittedHost))));
                }
            }
            else
            {
                WriteVerbose("No hosts configured, SNMP allowed from Any host!");
            }

            results.ToList().ForEach(WriteObject);

            base.ProcessRecord();
        }
    }
}