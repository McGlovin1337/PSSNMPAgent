using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using PSSNMPAgent.Remote;
using System.Text.RegularExpressions;

namespace RemoveSNMPCommunity.cmd
{
    [Cmdlet(VerbsCommon.Remove, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class RemoveSNMPCommunity: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Remove the specified SNMP Community Names")]
        public string[] Community { get; set; }

        [Parameter(Position = 1, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        private static IEnumerable<SNMPCommunity> _SNMPCommunities;

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                var Match = Regex.Match(Computer, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                if (!Match.Success)
                {
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Host);
                }
                WriteVerbose("Retrieving list of current SNMP Communities from Computer: " + Computer);
                _SNMPCommunities = SNMPRemote.RemoteGetSNMPCommunity(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retrieving list of current SNMP Communities...");
                _SNMPCommunities = SNMPAgentCommon.GetCommunities();
            }

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPCommunities;

            if (results.Count() == 0)
            {
                throw new Exception("There are no SNMP Community Names to remove");
            }

            results = results.Where(result => Community.Contains(result.Community));

            if (results.Count() == 0)
            {
                throw new Exception("None of the specified community names were found");                
            }

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Removing specified SNMP Community Names from Computer: " + Computer);
                SNMPRemote.RemoteRemoveSNMPCommunities(results, Computer, Credential);

                WriteVerbose("Retrieving list of current SNMP Communities from Computer: " + Computer);
                _SNMPCommunities = SNMPRemote.RemoteGetSNMPCommunity(Computer, Credential);
            }
            else
            {
                WriteVerbose("Removing specified SNMP Community Names...");
                RemoveSNMPCommunities(results);

                WriteVerbose("Retrieving list of current SNMP Communities...");
                _SNMPCommunities = SNMPAgentCommon.GetCommunities();
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPCommunities;

            results.ToList().ForEach(WriteObject);

            if (results.Count() > 0)
            {
                results = results.Where(result => Community.Contains(result.Community));
            }

            if (results.Count() == 0)
            {
                WriteVerbose("Successfully removed specified SNMP Community names");
            }
            else
            {
                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                {
                    WriteVerbose("Failed to remove the following SNMP Community names:");
                    foreach (var result in results)
                    {
                        WriteVerbose(result.Community);
                    }
                }
                throw new Exception("Failed to remove all specified SNMP Community names");
            }

            base.EndProcessing();
        }

        private static void RemoveSNMPCommunities(IEnumerable<SNMPCommunity> Communities)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegCommunities = Registry.LocalMachine.CreateSubKey(common.RegCommunities);

            foreach (var value in Communities)
            {
                if (value.Community != "(Default)")
                {
                    RegCommunities.DeleteValue(value.Community);
                }
            }
            RegCommunities.Close();
        }
    }
}