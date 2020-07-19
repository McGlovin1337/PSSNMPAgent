﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace GetSNMPTrap.cmd
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPTrap))]
    [OutputType(typeof(SNMPTrap))]
    public class GetSNMPTrap: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Specify Community name to match")]
        public string[] Community { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Specify Destination to match")]
        public string[] Destination { get; set; }

        private IEnumerable<SNMPTrap> _SNMPTrap;

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Destination"))
            {
                foreach (string Host in Destination)
                {
                    var Match = Regex.Match(Host, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                    if (!Match.Success)
                    {
                        throw new ArgumentException("Specified Destination is not a valid hostname: " + Host);
                    }
                }
            }
            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            WriteVerbose("Retrieving current SNMP Trap Communities and Destinations...");
            _SNMPTrap = SNMPAgentCommon.GetSNMPTraps();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPTrap;

            if (results.Count() > 0)
            {
                if (Community != null)
                {
                    results = results.Where(community => Community.Contains(community.Community));
                }

                if (Destination != null)
                {
                    results = results.Where(destination => Regex.IsMatch(destination.Destination, string.Format("^(?i:{0})", string.Join("|", Destination))));
                }
            }

            results.ToList().ForEach(WriteObject);

            base.ProcessRecord();
        }
    }
}