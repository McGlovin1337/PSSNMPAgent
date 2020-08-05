using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPHostCmdlets
{
    [Cmdlet(VerbsCommon.Remove, nameof(SNMPHost))]
    public class RemoveSNMPHost: BaseSNMPHost
    {
        [Parameter(Position = 0, ParameterSetName = "Default", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Remove SNMP Permitted Managers")]
        [Alias("Hosts", "Host", "Manager", "PermittedManager")]
        public string[] PermittedHost { get; set; }

        [Parameter(Position = 1, ParameterSetName = "RemoveAll", ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Remove all SNMP Permitted Managers")]
        public SwitchParameter RemoveAllHosts { get; set; }

        IEnumerable<string> _validHostsQuery;

        protected override void ProcessRecord()
        {
            if (MyInvocation.BoundParameters.ContainsKey("PermittedHost"))
            {
                WriteVerbose("Validating " + PermittedHost.Count() + " submitted PermittedHost hostnames...");
                IEnumerable<ValidateHostname> validatedHosts = ValidateHostname.ValidateHostnames(PermittedHost);

                IEnumerable<string> ValidHosts = validatedHosts.Where(valid => valid.ValidHostnames != null).Select(valid => valid.ValidHostnames);
                IEnumerable<string> InvalidHosts = validatedHosts.Where(valid => valid.InvalidHostnames != null).Select(valid => valid.InvalidHostnames);
                WriteVerbose("Yielded " + ValidHosts.Count() + " Valid Hostnames.");
                WriteVerbose("Yielded " + InvalidHosts.Count() + " Invalid Hostnames.");
                if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
                {
                    foreach (string invalidHost in InvalidHosts)
                        WriteVerbose(invalidHost + " is not a valid hostname!");
                }

                _validHostsQuery = ValidHosts;
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            _validHostsQuery = null;
        }

        protected override void ProcessSNMPHost(IEnumerable<SNMPHost> SNMPHosts)
        {
            // Prepare
            IEnumerable<string> ValidHostsQuery = _validHostsQuery;

            if (SNMPHosts.Count() == 0)
            {
                throw new Exception("Host list is empty, no hosts to remove");
            }

            if (!MyInvocation.BoundParameters.ContainsKey("RemoveAllHosts"))
            {
                string[] LowerHost = Array.ConvertAll(ValidHostsQuery.ToArray(), host => host.ToLower());

                SNMPHosts = SNMPHosts.Where(result => LowerHost.Contains(result.PermittedHost.ToLower()));

                if (SNMPHosts.Count() == 0)
                {
                    throw new Exception("None of the specified hosts were found");
                }
            }

            // Execute
            WriteVerbose("Removing specified SNMP Permitted Hosts...");
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                RemoteRemoveSNMPHosts(SNMPHosts, Computer, Credential);

                WriteVerbose("Retrieving list of current SNMP Hosts from Computer: " + Computer);
                SNMPHosts = RemoteGetSNMPHosts(Computer, Credential);
            }
            else
            {
                RemoveSNMPHosts(SNMPHosts);

                WriteVerbose("Retrieving list of current SNMP Hosts...");
                SNMPHosts = GetSNMPHosts();
            }

            // Confirm
            if (ValidHostsQuery != null)
            {
                string[] LowerHost = Array.ConvertAll(ValidHostsQuery.ToArray(), host => host.ToLower());

                if (SNMPHosts.Count() > 0)
                {
                    SNMPHosts = SNMPHosts.Where(host => LowerHost.Contains(host.PermittedHost.ToLower()));
                }
            }

            if (SNMPHosts.Count() == 0)
            {
                WriteObject("Specified hosts removed");
            }
            else
            {
                foreach (var host in SNMPHosts)
                {
                    WriteObject("Failed to remove host: " + host.PermittedHost);
                }
            }
        }

        private static void RemoveSNMPHosts(IEnumerable<SNMPHost> Hosts)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegHosts = Registry.LocalMachine.OpenSubKey(common.RegHosts);
            List<string> valueNames = new List<string>();
            string[] LowerHosts = Hosts.Select(host => host.PermittedHost).ToArray();
            LowerHosts = Array.ConvertAll(LowerHosts, host => host.ToLower());
            string hostname;

            foreach (string valueName in RegHosts.GetValueNames())
            {
                if (valueName != "(Default)")
                {
                    hostname = (string)RegHosts.GetValue(valueName);
                    if (LowerHosts.Contains(hostname.ToLower()))
                    {
                        valueNames.Add(valueName);
                    }
                }
            }
            RegHosts.Close();

            RegHosts = Registry.LocalMachine.CreateSubKey(common.RegHosts);
            foreach (string value in valueNames)
            {
                RegHosts.DeleteValue(value);
            }
            RegHosts.Close();
        }

        private static void RemoteRemoveSNMPHosts(IEnumerable<SNMPHost> Hosts, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            List<string> valueNames = new List<string>();
            string[] LowerHosts = Hosts.Select(host => host.PermittedHost).ToArray();
            LowerHosts = Array.ConvertAll(LowerHosts, host => host.ToLower());
            string hostname;

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] ValueNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            if (ValueNames == null) return;

            mboIn = mc.GetMethodParameters("GetStringValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            foreach (string valueName in ValueNames)
            {
                if (valueName != "(Default)")
                {
                    mboIn["sValueName"] = valueName;

                    mboOut = mc.InvokeMethod("GetStringValue", mboIn, null);
                    hostname = (string)mboOut["sValue"];
                    mboOut.Dispose();

                    if (LowerHosts.Contains(hostname.ToLower()))
                    {
                        valueNames.Add(valueName);
                    }
                }
            }
            mboIn.Dispose();

            mboIn = mc.GetMethodParameters("DeleteValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            foreach (string value in valueNames)
            {
                mboIn["sValueName"] = value;

                mc.InvokeMethod("DeleteValue", mboIn, null);
            }
            mboIn.Dispose();
        }
    }
}