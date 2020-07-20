using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;

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

        private static IEnumerable<SNMPCommunity> _SNMPCommunities;

        protected override void BeginProcessing()
        {
            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            WriteVerbose("Retrieving list of current SNMP Communities...");
            _SNMPCommunities = SNMPAgentCommon.GetCommunities();

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