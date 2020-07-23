using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using PSSNMPAgent.Remote;

namespace AddSNMPHost.cmd
{
    [Cmdlet(VerbsCommon.Add, nameof(SNMPHost))]
    [OutputType(typeof(SNMPHost))]

    public class AddSNMPHost : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Add a new SNMP Permitted Manager")]
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

            string[] LowerHost = Array.ConvertAll(PermittedHost, host => host.ToLower());

            if (results.Count() > 0)
            {
                results = results.Where(result => LowerHost.Contains(result.PermittedHost.ToLower()));
            }

            if (results.Count() > 0)
            {
                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                {
                    WriteVerbose("The following SNMP Hosts already exist:");
                    foreach (var result in results)
                    {
                        WriteVerbose(result.PermittedHost);
                    }
                }
                throw new Exception("SNMP Host already exists");
            }

            WriteVerbose("Adding " + PermittedHost.Count() + " hosts...");
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                SNMPRemote.RemoteAddSNMPHosts(PermittedHost, Computer, Credential);

                WriteVerbose("Retrieving list of current SNMP Hosts from Computer: " + Computer);
                _SNMPHosts = SNMPRemote.RemoteGetSNMPHosts(Computer, Credential);
            }
            else
            {
                AddSNMPHosts(PermittedHost);

                WriteVerbose("Retrieving list of current SNMP Hosts...");
                _SNMPHosts = SNMPAgentCommon.GetSNMPHosts();
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPHosts;
            string[] LowerHost = Array.ConvertAll(PermittedHost, host => host.ToLower());

            results = results.Where(result => LowerHost.Contains(result.PermittedHost.ToLower()));

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