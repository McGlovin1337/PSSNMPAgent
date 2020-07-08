using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace GetSNMPCommunity.Cmd
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class GetSNMPCommunity: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List matching SNMP Communities")]
        public string[] Communities { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "List SNMP Communities by Access Right")]
        [ValidateSet("None", "Notify", "ReadOnly", "ReadWrite", "ReadCreate")]
        public string[] AccessRights { get; set; }

        private static IEnumerable<SNMPCommunity> _SNMPCommunities;

        protected override void BeginProcessing()
        {
            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            _SNMPCommunities = GetCommunities();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPCommunities;

            if (results.Count() > 0)
            {
                if (Communities != null)
                {
                    results = results.Where(community => Regex.IsMatch(community.Community, string.Format("^(?:{0})", string.Join("|", Communities))));
                }

                if (MyInvocation.BoundParameters.ContainsKey("AccessRights"))
                {
                    results = results.Where(access => Regex.IsMatch(access.AccessRights, string.Format("^(?:{0})", string.Join("|", AccessRights))));
                }
            }
            else
            {
                WriteVerbose("No SNMP Community names configured");
            }

            results.ToList().ForEach(WriteObject);           

            base.ProcessRecord();
        }

        private static IEnumerable<SNMPCommunity> GetCommunities()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegCommunities = Registry.LocalMachine.OpenSubKey(common.RegCommunities);

            List<SNMPCommunity> communities = new List<SNMPCommunity>();
            
            foreach (string Community in RegCommunities.GetValueNames())
            {
                int accessValue = (int)RegCommunities.GetValue(Community);
                var accessType = common.CommunityAccess.Single(a => a.dWordVal == accessValue);
                string access = accessType.Access;
                communities.Add(new SNMPCommunity { Community = Community, AccessRights = access });
            }
            RegCommunities.Close();

            return communities;
        }        
    }
}