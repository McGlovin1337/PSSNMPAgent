using System;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;

namespace ResetSNMPAgent.cmd
{
    [Cmdlet(VerbsCommon.Reset, "SNMPAgent")]

    public class PSSNMPAgent: PSCmdlet
    {
        protected override void BeginProcessing()
        {
            WriteVerbose("Checking SNMP Service is installed...");
            SNMPAgentCommon.ServiceCheck();

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose("Resetting SNMP Agent Configuration to installation defaults...");
            ResetSNMPAgent();

            base.ProcessRecord();
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
            rfc1156Key.SetValue("sysServices", "76");
            rfc1156Key.Close();

            rootKey.SetValue("EnableAuthenticationTraps", "1");
            rootKey.SetValue("NameResolutionRetries", "16");
            rootKey.Close();
        }
    }
}