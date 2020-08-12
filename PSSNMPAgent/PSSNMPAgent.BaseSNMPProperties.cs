using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPPropertyCmdlets
{
    public class BaseSNMPProperties: PSCmdlet
    {
        [Parameter(Position = 0, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 1, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }
        protected virtual void ProcessSNMPProperty(IEnumerable<SNMPProperties> SNMPProperties)
        {
            throw new NotImplementedException("ProcessSNMPProperty Method should be overriden");
        }

        protected override void ProcessRecord()
        {
            IEnumerable<SNMPProperties> SNMPProperties;

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Validating " + Computer + " is a valid hostname...");
                bool ValidateComputer = ValidateHostname.ValidateHost(Computer);
                WriteVerbose("Validation result is " + ValidateComputer);

                if (ValidateComputer == false)
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Computer);

                WriteVerbose("Retrieving current SNMP Properties from Computer: " + Computer);
                SNMPProperties = RemoteGetSNMPProperties(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retriving current SNMP Properties...");
                SNMPProperties = GetSNMPProperties();
            }

            ProcessSNMPProperty(SNMPProperties);
        }

        protected static IEnumerable<SNMPProperties> GetSNMPProperties()
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

            bool SvcPhy, SvcDat, SvcInt, SvcEnd, SvcApp;

            const int _svcPhy = 1;
            const int _svcDat = 2;
            const int _svcInt = 4;
            const int _svcEnd = 8;
            const int _svcApp = 64;

            SvcPhy = ((SvcValue & _svcPhy) > 0) ? true : false;
            SvcDat = ((SvcValue & _svcDat) > 0) ? true : false;
            SvcInt = ((SvcValue & _svcInt) > 0) ? true : false;
            SvcEnd = ((SvcValue & _svcEnd) > 0) ? true : false;
            SvcApp = ((SvcValue & _svcApp) > 0) ? true : false;

            properties.Add(new SNMPProperties
            {
                EnableAuthTraps = AuthTraps,
                NameResolutionRetries = NameRetries,
                SysContact = Contact,
                SysLocation = Location,
                SvcPhysical = SvcPhy,
                SvcDatalink = SvcDat,
                SvcInternet = SvcInt,
                SvcEndToEnd = SvcEnd,
                SvcApplications = SvcApp
            });

            return properties;
        }

        protected static IEnumerable<SNMPProperties> RemoteGetSNMPProperties(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            List<SNMPProperties> properties = new List<SNMPProperties>();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("GetDWORDValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegRootSubKey;
            mboIn["sValueName"] = "EnableAuthenticationTraps";

            ManagementBaseObject mboIn2 = mc.GetMethodParameters("GetDWORDValue");
            mboIn2["hDefKey"] = (UInt32)2147483650;
            mboIn2["sSubKeyName"] = common.RegRootSubKey;
            mboIn2["sValueName"] = "NameResolutionRetries";

            ManagementBaseObject mboIn3 = mc.GetMethodParameters("GetDWORDValue");
            mboIn3["hDefKey"] = (UInt32)2147483650;
            mboIn3["sSubKeyName"] = common.RegRFC1156;
            mboIn3["sValueName"] = "sysServices";

            ManagementBaseObject mboIn4 = mc.GetMethodParameters("GetStringValue");
            mboIn4["hDefKey"] = (UInt32)2147483650;
            mboIn4["sSubKeyName"] = common.RegRFC1156;
            mboIn4["sValueName"] = "sysContact";

            ManagementBaseObject mboIn5 = mc.GetMethodParameters("GetStringValue");
            mboIn5["hDefKey"] = (UInt32)2147483650;
            mboIn5["sSubKeyName"] = common.RegRFC1156;
            mboIn5["sValueName"] = "sysLocation";

            ManagementBaseObject mboOut = mc.InvokeMethod("GetDWORDValue", mboIn, null);
            ManagementBaseObject mboOut2 = mc.InvokeMethod("GetDWORDValue", mboIn2, null);
            ManagementBaseObject mboOut3 = mc.InvokeMethod("GetDWORDValue", mboIn3, null);
            ManagementBaseObject mboOut4 = mc.InvokeMethod("GetStringValue", mboIn4, null);
            ManagementBaseObject mboOut5 = mc.InvokeMethod("GetStringValue", mboIn5, null);

            bool AuthTraps = Convert.ToBoolean((UInt32)mboOut["uValue"]);
            int NameRetries = Convert.ToInt32((UInt32)mboOut2["uValue"]);
            string Contact = (string)mboOut4["sValue"];
            string Location = (string)mboOut5["sValue"];
            int SvcValue = Convert.ToInt32((UInt32)mboOut3["uValue"]);

            mboIn.Dispose();
            mboIn2.Dispose();
            mboIn3.Dispose();
            mboIn4.Dispose();
            mboIn5.Dispose();
            mboOut.Dispose();
            mboOut2.Dispose();
            mboOut3.Dispose();
            mboOut4.Dispose();
            mboOut5.Dispose();

            bool SvcPhy, SvcDat, SvcInt, SvcEnd, SvcApp;

            const int _svcPhy = 1;
            const int _svcDat = 2;
            const int _svcInt = 4;
            const int _svcEnd = 8;
            const int _svcApp = 64;

            SvcPhy = ((SvcValue & _svcPhy) > 0) ? true : false;
            SvcDat = ((SvcValue & _svcDat) > 0) ? true : false;
            SvcInt = ((SvcValue & _svcInt) > 0) ? true : false;
            SvcEnd = ((SvcValue & _svcEnd) > 0) ? true : false;
            SvcApp = ((SvcValue & _svcApp) > 0) ? true : false;

            properties.Add(new SNMPProperties
            {
                EnableAuthTraps = AuthTraps,
                NameResolutionRetries = NameRetries,
                SysContact = Contact,
                SysLocation = Location,
                SvcPhysical = SvcPhy,
                SvcDatalink = SvcDat,
                SvcInternet = SvcInt,
                SvcEndToEnd = SvcEnd,
                SvcApplications = SvcApp
            });

            return properties;
        }

        protected class SNMPProperties
        {
            public bool EnableAuthTraps { get; set; }
            public int NameResolutionRetries { get; set; }
            public string SysContact { get; set; }
            public string SysLocation { get; set; }
            public bool SvcPhysical { get; set; }
            public bool SvcApplications { get; set; }
            public bool SvcDatalink { get; set; }
            public bool SvcInternet { get; set; }
            public bool SvcEndToEnd { get; set; }
        }
    }
}