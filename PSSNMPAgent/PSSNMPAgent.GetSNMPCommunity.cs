using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using PSSNMPAgent.Remote;

namespace GetSNMPCommunity.Cmd
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class GetSNMPCommunity: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List matching SNMP Communities")]
        public string[] Community { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "List SNMP Communities by Access Right")]
        [ValidateSet("None", "Notify", "ReadOnly", "ReadWrite", "ReadCreate")]
        public string[] AccessRight { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 3, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
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

                WriteVerbose("Retrieving list of current SNMP Community Names from Computer: " + Computer);
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

            if (results.Count() > 0)
            {
                if (Community != null)
                {                    
                    results = results.Where(community => Community.Contains(community.Community));
                }

                if (MyInvocation.BoundParameters.ContainsKey("AccessRights"))
                {
                    results = results.Where(access => AccessRight.Contains(access.AccessRight));
                }
            }
            else
            {
                WriteVerbose("No SNMP Community names configured");
            }

            results.ToList().ForEach(WriteObject);           

            base.ProcessRecord();
        }      
    }
}