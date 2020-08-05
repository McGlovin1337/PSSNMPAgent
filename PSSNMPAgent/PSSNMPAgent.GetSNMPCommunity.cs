using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSSNMPAgent.SNMPCommunityCmdlets
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class GetSNMPCommunity: BaseSNMPCommunity
    {
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Specify SNMP Community Names by Access Right")]
        [ValidateSet("None", "Notify", "ReadOnly", "ReadWrite", "ReadCreate")]
        public string[] AccessRight { get; set; }

        protected override void ProcessSNMPCommunity(IEnumerable<SNMPCommunity> SNMPCommunities)
        {
            if (SNMPCommunities.Count() > 0)
            {
                if (Community != null)
                {
                    SNMPCommunities = SNMPCommunities.Where(community => Community.Contains(community.Community));
                }

                if (MyInvocation.BoundParameters.ContainsKey("AccessRight"))
                {
                    SNMPCommunities = SNMPCommunities.Where(access => AccessRight.Contains(access.AccessRight));
                }
            }
            else
            {
                WriteVerbose("No SNMP Community names configured");
            }

            SNMPCommunities.ToList().ForEach(WriteObject);
        }
    }
}