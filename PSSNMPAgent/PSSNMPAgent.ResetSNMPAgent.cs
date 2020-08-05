using System;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace ResetSNMPAgent.cmd
{
    [Cmdlet(VerbsCommon.Reset, "SNMPAgent")]

    public class PSSNMPAgent: PSCmdlet
    {
        [Parameter(Position = 0, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 1, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        protected override void ProcessRecord()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Validating " + Computer + " is a valid hostname...");
                bool ValidateComputer = ValidateHostname.ValidateHost(Computer);
                WriteVerbose("Validation result is " + ValidateComputer);

                if (ValidateComputer == false)
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Computer);

                WriteVerbose("Resetting SNMP Agent Configuration to installation defaults on Computer: " + Computer);
                RemoteResetSNMPAgent(Computer, Credential);
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();

                WriteVerbose("Resetting SNMP Agent Configuration to installation defaults...");
                ResetSNMPAgent();
            }
        }

        private static void ResetSNMPAgent()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey rootKey = Registry.LocalMachine.CreateSubKey(common.RegRootSubKey);
            RegistryKey communityKey = Registry.LocalMachine.CreateSubKey(common.RegCommunities);
            RegistryKey hostsKey = Registry.LocalMachine.CreateSubKey(common.RegHosts);
            RegistryKey trapsKey = Registry.LocalMachine.CreateSubKey(common.RegTraps);
            RegistryKey rfc1156Key = Registry.LocalMachine.CreateSubKey(common.RegRFC1156);

            foreach (string value in communityKey.GetValueNames())
            {
                if (value != "(Default)") communityKey.DeleteValue(value);
            }
            communityKey.Close();

            foreach (string value in hostsKey.GetValueNames())
            {
                if (value != "(Default)") hostsKey.DeleteValue(value);
            }
            hostsKey.SetValue("1", "localhost");
            hostsKey.Close();

            foreach (string valueName in trapsKey.GetSubKeyNames())
            {                
                trapsKey.DeleteSubKey(valueName);
            }
            trapsKey.Close();

            rfc1156Key.SetValue("sysContact", "");
            rfc1156Key.SetValue("sysLocation", "");
            rfc1156Key.SetValue("sysServices", "76", RegistryValueKind.DWord);
            rfc1156Key.Close();

            rootKey.SetValue("EnableAuthenticationTraps", "1", RegistryValueKind.DWord);
            rootKey.SetValue("NameResolutionRetries", "16", RegistryValueKind.DWord);
            rootKey.Close();
        }

        private static void RemoteResetSNMPAgent(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] valueNames = (string[])mboOut["sNames"];
            mboIn.Dispose();
            mboOut.Dispose();

            if (valueNames != null)
            {
                mboIn = mc.GetMethodParameters("DeleteValue");
                mboIn["hDefKey"] = (UInt32)2147483650;
                mboIn["sSubKeyName"] = common.RegCommunities;

                foreach (string value in valueNames)
                {
                    mboIn["sValueName"] = value;
                    if (value != "(Default)") mc.InvokeMethod("DeleteValue", mboIn, null);
                }
                mboIn.Dispose();
                Array.Clear(valueNames, 0, valueNames.Length);
            }

            mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            valueNames = (string[])mboOut["sNames"];
            mboIn.Dispose();
            mboOut.Dispose();

            if (valueNames != null)
            {
                mboIn = mc.GetMethodParameters("DeleteValue");
                mboIn["hDefKey"] = (UInt32)2147483650;
                mboIn["sSubKeyName"] = common.RegHosts;

                foreach (string value in valueNames)
                {
                    mboIn["sValueName"] = value;
                    if (value != "(Default)") mc.InvokeMethod("DeleteValue", mboIn, null);
                }
                mboIn.Dispose();
                Array.Clear(valueNames, 0, valueNames.Length);
            }

            mboIn = mc.GetMethodParameters("SetStringValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;
            mboIn["sValueName"] = "1";
            mboIn["sValue"] = "localhost";

            mc.InvokeMethod("SetStringValue", mboIn, null);
            mboIn.Dispose();

            mboIn = mc.GetMethodParameters("EnumKey");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegTraps;

            mboOut = mc.InvokeMethod("EnumKey", mboIn, null);
            valueNames = (string[])mboOut["sNames"];
            mboIn.Dispose();
            mboOut.Dispose();

            if (valueNames != null)
            {
                mboIn = mc.GetMethodParameters("DeleteKey");
                mboIn["hDefKey"] = (UInt32)2147483650;

                foreach (string valueName in valueNames)
                {
                    mboIn["sSubKeyName"] = common.RegTraps + @"\" + valueName;
                    mc.InvokeMethod("DeleteKey", mboIn, null);
                }
                mboIn.Dispose();
            }

            mboIn = mc.GetMethodParameters("SetStringValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegRFC1156;
            mboIn["sValueName"] = "sysContact";
            mboIn["sValue"] = "";
            mc.InvokeMethod("SetStringValue", mboIn, null);

            mboIn["sValueName"] = "sysLocation";
            mboIn["sValue"] = "";
            mc.InvokeMethod("SetStringValue", mboIn, null);
            mboIn.Dispose();

            mboIn = mc.GetMethodParameters("SetDWORDValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegRFC1156;
            mboIn["sValueName"] = "sysServices";
            mboIn["uValue"] = 76;
            mc.InvokeMethod("SetDWORDValue", mboIn, null);

            mboIn["sSubKeyName"] = common.RegRootSubKey;
            mboIn["sValueName"] = "EnableAuthenticationTraps";
            mboIn["uValue"] = 1;
            mc.InvokeMethod("SetDWORDValue", mboIn, null);

            mboIn["sValueName"] = "NameResolutionRetries";
            mboIn["uValue"] = 16;
            mc.InvokeMethod("SetDWORDValue", mboIn, null);

            mboIn.Dispose();
        }
    }
}