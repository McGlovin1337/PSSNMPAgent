using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;

namespace SetSNMPProperties.cmd
{
    [Cmdlet(VerbsCommon.Set, nameof(SNMPProperties))]
    [OutputType(typeof(SNMPProperties))]

    public class PSSNMPAgent: PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Set System Contact Details")]
        [AllowNull, AllowEmptyString]
        public string SysContact { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Set System Location Details")]
        [AllowNull, AllowEmptyString]
        public string SysLocation { get; set; }

        [Parameter(Position = 2, HelpMessage = "Enable/Disable Authentication Traps")]
        public SwitchParameter EnableAuthTraps { get; set; }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, HelpMessage = "Number of Name Resolution Retries")]
        public int NameResolutionRetries { get; set; }

        [Parameter(Position = 4, HelpMessage = "Enable/Disable Service: Physical")]
        public SwitchParameter SvcPhysical { get; set; }

        [Parameter(Position = 5, HelpMessage = "Enable/Disable Service: Applications")]
        public SwitchParameter SvcApplications { get; set; }

        [Parameter(Position = 6, HelpMessage = "Enable/Disable Service: Datalink and Subnetwork")]
        public SwitchParameter SvcDatalink { get; set; }

        [Parameter(Position = 7, HelpMessage = "Enable/Disable Service: Internet")]
        public SwitchParameter SvcInternet { get; set; }

        [Parameter(Position = 8, HelpMessage = "Enable/Disable Service: End-to-End")]
        public SwitchParameter SvcEndToEnd { get; set; }

        private IEnumerable<SNMPProperties> _SNMPProperties;        

        protected override void BeginProcessing()
        {
            WriteVerbose("Retrieving current SNMP Properties...");
            _SNMPProperties = SNMPAgentCommon.GetSNMPProperties();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPProperties;

            SNMPAgentCommon common = new SNMPAgentCommon();

            RegistryKey RegRoot = Registry.LocalMachine.CreateSubKey(common.RegRootSubKey);
            RegistryKey RegRFC = Registry.LocalMachine.CreateSubKey(common.RegRFC1156);

            int sysSvcValue = (int)RegRFC.GetValue("sysServices");
            int origSvcValue = sysSvcValue;

            if (MyInvocation.BoundParameters.ContainsKey("SysContact"))
            {
                RegRFC.SetValue("SysContact", SysContact);
            }

            if (MyInvocation.BoundParameters.ContainsKey("SysLocation"))
            {
                RegRFC.SetValue("SysLocation", SysLocation);
            }

            if (MyInvocation.BoundParameters.ContainsKey("EnableAuthTraps"))
            {
                RegRoot.SetValue("EnableAuthenticationTraps", EnableAuthTraps);
            }

            if (MyInvocation.BoundParameters.ContainsKey("NameResolutionRetries"))
            {
                RegRoot.SetValue("NameResolutionRetries", NameResolutionRetries);
            }

            if (MyInvocation.BoundParameters.ContainsKey("SvcPhysical"))
            {
                foreach (var result in results)
                {
                    if (!SvcPhysical == result.SvcPhysical)
                    {
                        sysSvcValue = (SvcPhysical == true) ? sysSvcValue + 1 : sysSvcValue - 1;
                    }
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey("SvcApplications"))
            {
                foreach (var result in results)
                {
                    if (!SvcApplications == result.SvcApplications)
                    {
                        sysSvcValue = (SvcApplications == true) ? sysSvcValue + 64 : sysSvcValue - 64;
                    }
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey("SvcDatalink"))
            {
                foreach (var result in results)
                {
                    if (!SvcDatalink == result.SvcDatalink)
                    {
                        sysSvcValue = (SvcDatalink == true) ? sysSvcValue + 2 : sysSvcValue - 2;
                    }
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey("SvcInternet"))
            {
                foreach (var result in results)
                {
                    if (!SvcInternet == result.SvcInternet)
                    {
                        sysSvcValue = (SvcInternet == true) ? sysSvcValue + 4 : sysSvcValue - 4;
                    }
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey("SvcEndToEnd"))
            {
                foreach (var result in results)
                {
                    if (!SvcEndToEnd == result.SvcEndToEnd)
                    {
                        sysSvcValue = (SvcEndToEnd == true) ? sysSvcValue + 8 : sysSvcValue - 8;
                    }
                }
            }

            if (sysSvcValue != origSvcValue)
            {
                RegRFC.SetValue("sysServices", sysSvcValue);
            }

            _SNMPProperties = SNMPAgentCommon.GetSNMPProperties();

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPProperties;

            results.ToList().ForEach(WriteObject);

            base.EndProcessing();
        }
    }
}