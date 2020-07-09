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

            _SNMPProperties = GetSNMPProperties();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            var results = _SNMPProperties;

            results.ToList().ForEach(WriteObject);

            base.ProcessRecord();
        }

        private static IEnumerable<SNMPProperties> GetSNMPProperties()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegRoot = Registry.LocalMachine.OpenSubKey(common.RegRootSubKey);
            RegistryKey RegRFC = Registry.LocalMachine.OpenSubKey(common.RegRFC1156);

            List<SNMPProperties> properties = new List<SNMPProperties>();

            bool AuthTraps = Convert.ToBoolean((int)RegRoot.GetValue("EnableAuthenticationTraps"));
            int NameRetries = (int)RegRoot.GetValue("NameResolutionRetries");
            string Contact = (string)RegRFC.GetValue("sysContact");
            string Location = (string)RegRFC.GetValue("sysLocation");
            int SvcValue = (int)RegRFC.GetValue("sysServices");

            RegRFC.Close();
            RegRoot.Close();

            bool SvcPhy = false, SvcDat = false, SvcInt = false, SvcEnd = false, SvcApp = false;

            if (SvcValue >= 64)
            {
                SvcApp = true;
                SvcValue = SvcValue - 64;
            }
            SvcEnd = (SvcValue >= 8 && SvcValue < 64) ? true : false;
            if (SvcValue == 1 || SvcValue == 9) { SvcPhy = true; }
            if (SvcValue == 2 || SvcValue == 10) { SvcDat = true; }
            if (SvcValue == 4 || SvcValue == 12) { SvcInt = true; }
            if (SvcValue == 3 || SvcValue == 11) { SvcPhy = true; SvcDat = true; }
            if (SvcValue == 5 || SvcValue == 13) { SvcPhy = true; SvcInt = true; }
            if (SvcValue == 6 || SvcValue == 14) { SvcDat = true; SvcInt = true; }
            if (SvcValue == 7 || SvcValue == 15) { SvcPhy = true; SvcDat = true; SvcInt = true; }
            
            properties.Add(new SNMPProperties { 
                EnableAuthTraps = AuthTraps, 
                NameResolutionRetries = NameRetries, 
                SysContact = Contact,
                SysLocation = Location,
                SvcPhysical = SvcPhy,
                SvcDatalink = SvcDat,
                SvcInternet = SvcInt,
                SvcEndToEnd = SvcEnd,
                SvcApplications = SvcApp });

            return properties;
        }
    }
}