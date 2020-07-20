﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;

namespace AddSNMPCommunity.cmd
{
    [Cmdlet(VerbsCommon.Add, nameof(SNMPCommunity))]
    [OutputType(typeof(SNMPCommunity))]
    public class PSSNMPAgent: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Add new SNMP Community Name")]
        public string[] Community { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Add new SNMP Community with the given Access Rights")]
        [ValidateSet("None", "Notify", "ReadOnly", "ReadWrite", "ReadCreate")]
        public string AccessRight { get; set; }

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
                results = results.Where(result => Community.Contains(result.Community));
            }

            if (results.Count() > 0)
            {
                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                {
                    WriteVerbose("The following SNMP Community names already exist:");
                    foreach (var result in results)
                    {
                        WriteVerbose(result.Community);
                    }
                }
                throw new Exception("SNMP Community already exists");
            }

            WriteVerbose("Adding " + Community.Count() + " Communities...");
            AddCommunities(Community, AccessRight);

            WriteVerbose("Retrieving list of current SNMP Communities...");
            _SNMPCommunities = SNMPAgentCommon.GetCommunities();

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPCommunities;

            if (results.Count() > 0)
            {
                results = results.Where(result => Community.Contains(result.Community));
            }

            if (results.Count() == Community.Count())
            {
                WriteVerbose("Successfully added all SNMP Community Names");
            }
            else if (results.Count() > 0)
            {
                string[] communityArr = results.Select(result => result.Community).ToArray();
                WriteVerbose("Failed to add the following SNMP Communities...");
                foreach (string communityName in Community)
                {
                    bool Match = communityArr.Contains(communityName);
                    if (Match == false)
                    {
                        WriteVerbose(communityName);
                    }
                }
            }
            else
            {
                throw new Exception("Failed to add specified SNMP Community Names");
            }

            results.ToList().ForEach(WriteObject);

            base.EndProcessing();
        }

        private static void AddCommunities(string[] Communities, string Access)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegCommunities = Registry.LocalMachine.CreateSubKey(common.RegCommunities);
            int AccessValue = common.CommunityAccess.Single(val => val.Access == Access).dWordVal;

            foreach (string Community in Communities)
            {
                RegCommunities.SetValue(Community, AccessValue);
            }
            RegCommunities.Close();
        }
    }
}