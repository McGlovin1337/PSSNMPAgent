using System;
using System.Collections.Generic;
using System.Linq;
using PSSNMPAgent.Common;
using System.Management.Automation;
using Microsoft.Win32;

namespace GetSNMPProperty.cmd
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPProperties))]
    [OutputType(typeof(SNMPProperties))]

    public class GetSNMPProperty: PSCmdlet
    {
        private IEnumerable<SNMPProperties> _SNMPProperties;

        protected override void BeginProcessing()
        {
            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            WriteVerbose("Retriving current SNMP Properties...");
            _SNMPProperties = SNMPAgentCommon.GetSNMPProperties();

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