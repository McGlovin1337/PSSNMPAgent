using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;

namespace RemoveSNMPCommunity.cmd
{
    [Cmdlet(VerbsCommon.Remove, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class RemoveSNMPCommunity: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Remove the specified SNMP Community Names")]
        public string[] Communities { get; set; }

        private static IEnumerable<SNMPCommunity> _SNMPCommunities;

        protected override void BeginProcessing()
        {
            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            _SNMPCommunities = SNMPAgentCommon.GetCommunities();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPCommunities;

            if (results.Count() == 0)
            {
                throw new Exception("There are no SNMP Community Names to remove");
            }

            results = results.Where(result => Communities.Contains(result.Community));

            if (results.Count() == 0)
            {
                throw new Exception("None of the specified community names were found");                
            }

            RemoveSNMPCommunities(results);

            _SNMPCommunities = SNMPAgentCommon.GetCommunities();

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPCommunities;

            results.ToList().ForEach(WriteObject);

            if (results.Count() > 0)
            {
                results = results.Where(result => Communities.Contains(result.Community));
            }

            if (results.Count() == 0)
            {
                WriteVerbose("Successfully removed specified SNMP Community names");
            }
            else
            {
                WriteVerbose("Failed to remove the following SNMP Community names:");
                foreach (var result in results)
                {
                    WriteVerbose(result.Community);
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