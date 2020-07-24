using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using PSSNMPAgent.Remote;

namespace RemoveSNMPHost.cmd
{
    [Cmdlet(VerbsCommon.Remove, nameof(SNMPHost))]
    public class RemoveSNMPHost : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Remove SNMP Permitted Managers")]
        [Alias("Hosts", "Host", "Manager", "PermittedManager")]
        public string[] PermittedHost { get; set; }

        [Parameter(Position = 1, ParameterSetName = "RemoveAll", ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Remove all SNMP Permitted Managers")]
        public SwitchParameter RemoveAllHosts { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 3, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
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
                        throw new ArgumentException("Specified Host is not a valid hostname: " + Host);
                    }
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                var Match = Regex.Match(Computer, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                if (!Match.Success)
                {
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Host);
                }
                WriteVerbose("Retrieving list of current SNMP Hosts from Computer: " + Computer);
                _SNMPHosts = SNMPRemote.RemoteGetSNMPHosts(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retrieving list of current SNMP Hosts...");
                _SNMPHosts = SNMPAgentCommon.GetSNMPHosts();
            }

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPHosts;

            if (results.Count() == 0)
            {
                throw new Exception("Host list is empty, no hosts to remove");
            }

            if (!MyInvocation.BoundParameters.ContainsKey("RemoveAllHosts"))
            {
                string[] LowerHost = Array.ConvertAll(PermittedHost, host => host.ToLower());

                results = results.Where(result => LowerHost.Contains(result.PermittedHost.ToLower()));

                if (results.Count() == 0)
                {
                    throw new Exception("None of the specified hosts were found");
                }
            }

            WriteVerbose("Removing specified SNMP Permitted Hosts...");
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                SNMPRemote.RemoteRemoveSNMPHosts(results, Computer, Credential);

                WriteVerbose("Retrieving list of current SNMP Hosts from Computer: " + Computer);
                _SNMPHosts = SNMPRemote.RemoteGetSNMPHosts(Computer, Credential);
            }
            else
            {
                RemoveSNMPHosts(results);

                WriteVerbose("Retrieving list of current SNMP Hosts...");
                _SNMPHosts = SNMPAgentCommon.GetSNMPHosts();
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPHosts;

            results.ToList().ForEach(WriteObject);

            if (MyInvocation.BoundParameters.ContainsKey("PermittedHost"))
            {
                string[] LowerHost = Array.ConvertAll(PermittedHost, host => host.ToLower());

                if (results.Count() > 0)
                {
                    results = results.Where(result => LowerHost.Contains(result.PermittedHost.ToLower()));
                }
            }

            if (results.Count() == 0)
            {
                WriteVerbose("Specified hosts removed");
            }
            else
            {
                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                {
                    WriteVerbose("Failed to remove the following hosts:");
                    foreach (var result in results)
                    {
                        WriteVerbose(result.PermittedHost);
                    }
                }
                throw new Exception("Some hosts failed to remove");
            }

            base.EndProcessing();
        }

        private static void RemoveSNMPHosts(IEnumerable<SNMPHost> Hosts)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegHosts = Registry.LocalMachine.OpenSubKey(common.RegHosts);
            List<string> valueNames = new List<string>();
            string[] LowerHosts = Hosts.Select(host => host.PermittedHost).ToArray();
            LowerHosts = Array.ConvertAll(LowerHosts, host => host.ToLower());
            string hostname;

            foreach (string valueName in RegHosts.GetValueNames())
            {
                if (valueName != "(Default)")
                {
                    hostname = (string)RegHosts.GetValue(valueName);
                    if (LowerHosts.Contains(hostname.ToLower()))
                    {
                        valueNames.Add(valueName);
                    }
                }
            }
            RegHosts.Close();

            RegHosts = Registry.LocalMachine.CreateSubKey(common.RegHosts);
            foreach (string value in valueNames)
            {
                RegHosts.DeleteValue(value);
            }
            RegHosts.Close();
        }
    }
}