using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPHostCmdlets
{
    public class BaseSNMPHost: PSCmdlet
    {
        [Parameter(Position = 1, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        protected virtual void ProcessSNMPHost(IEnumerable<SNMPHost> SNMPHosts)
        {
            throw new NotImplementedException("ProcessSNMPHost Method should be overriden");
        }

        protected override void ProcessRecord()
        {
            IEnumerable<SNMPHost> SNMPHosts;

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Validating " + Computer + " is a valid hostname...");
                bool ValidateComputer = ValidateHostname.ValidateHost(Computer);
                WriteVerbose("Validation result is " + ValidateComputer);

                if (ValidateComputer == false)
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Computer);

                WriteVerbose("Retrieving list of current SNMP Permitted Hosts from Computer: " + Computer);
                SNMPHosts = RemoteGetSNMPHosts(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Retrieving list of current SNMP Permitted Hosts...");
                SNMPHosts = GetSNMPHosts();
            }

            ProcessSNMPHost(SNMPHosts);
        }

        protected class SNMPHost
        {
            public string PermittedHost { get; set; }
        }

        protected static IEnumerable<SNMPHost> GetSNMPHosts()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegHosts = Registry.LocalMachine.OpenSubKey(common.RegHosts);

            List<SNMPHost> hosts = new List<SNMPHost>();

            foreach (string value in RegHosts.GetValueNames())
            {
                string host = (string)RegHosts.GetValue(value);
                hosts.Add(new SNMPHost { PermittedHost = host });
            }
            RegHosts.Close();

            return hosts;
        }

        protected static IEnumerable<SNMPHost> RemoteGetSNMPHosts(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] ValueNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            List<SNMPHost> hosts = new List<SNMPHost>();

            if (ValueNames == null) return hosts;

            foreach (var value in ValueNames)
            {
                mboIn = mc.GetMethodParameters("GetStringValue");
                mboIn["hDefKey"] = (UInt32)2147483650;
                mboIn["sSubKeyName"] = common.RegHosts;
                mboIn["sValueName"] = value;

                mboOut = mc.InvokeMethod("GetStringValue", mboIn, null);
                hosts.Add(new SNMPHost { PermittedHost = (string)mboOut["sValue"] });

                mboIn.Dispose();
                mboOut.Dispose();
            }

            return hosts;
        }
    }
}
