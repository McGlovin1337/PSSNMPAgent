using System;
using System.Collections.Generic;
using System.Linq;
using PSSNMPAgent.Common;
using System.Management.Automation;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using PSSNMPAgent.Remote;

namespace GetSNMPProperties.cmd
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPProperties))]
    [OutputType(typeof(SNMPProperties))]

    public class GetSNMPProperty: PSCmdlet
    {
        [Parameter(Position = 0, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 1, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        private IEnumerable<SNMPProperties> _SNMPProperties;

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                var Match = Regex.Match(Computer, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                if (!Match.Success)
                {
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Host);
                }

                WriteVerbose("Retrieving current SNMP Properties from Computer: " + Computer);
                _SNMPProperties = SNMPRemote.RemoteGetSNMPProperties(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retriving current SNMP Properties...");
                _SNMPProperties = SNMPAgentCommon.GetSNMPProperties();
            }

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPProperties;

            results.ToList().ForEach(WriteObject);

            base.ProcessRecord();
        }        
    }
}