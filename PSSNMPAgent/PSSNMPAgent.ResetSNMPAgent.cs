using System;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using PSSNMPAgent.Remote;
using System.Text.RegularExpressions;

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

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                var Match = Regex.Match(Computer, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
                if (!Match.Success)
                {
                    throw new ArgumentException("Specified Computer is not a valid hostname: " + Host);
                }
            }
            else
            {
                WriteVerbose("Checking SNMP Service is installed...");
                SNMPAgentCommon.ServiceCheck();
            }

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Resetting SNMP Agent Configuration to installation defaults on Computer: " + Computer);
                SNMPRemote.RemoteResetSNMPAgent(Computer, Credential);
            }
            else
            {
                WriteVerbose("Resetting SNMP Agent Configuration to installation defaults...");
                ResetSNMPAgent();
            }

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
            rfc1156Key.SetValue("sysServices", "76", RegistryValueKind.DWord);
            rfc1156Key.Close();

            rootKey.SetValue("EnableAuthenticationTraps", "1", RegistryValueKind.DWord);
            rootKey.SetValue("NameResolutionRetries", "16", RegistryValueKind.DWord);
            rootKey.Close();
        }
    }
}