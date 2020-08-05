using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using System.Management;

namespace PSSNMPAgent.SNMPHostCmdlets
{
    [Cmdlet(VerbsCommon.Add, nameof(SNMPHost))]
    [OutputType(typeof(SNMPHost))]

    public class AddSNMPHost: BaseSNMPHost
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Add a new SNMP Permitted Manager")]
        [Alias("Hosts", "Host", "Manager", "PermittedManager")]
        public string[] PermittedHost { get; set; }

        private static IEnumerable<string> _validHostsQuery;

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
            string[] validHostsQuery = _validHostsQuery.ToArray();

            string[] LowerHost = Array.ConvertAll(validHostsQuery, host => host.ToLower());

            if (SNMPHosts.Count() > 0)
            {
                SNMPHosts = SNMPHosts.Where(result => LowerHost.Contains(result.PermittedHost.ToLower()));
            }

            if (SNMPHosts.Count() > 0)
            {
                WriteVerbose("Hosts to be added already exist");

                foreach (var hostExist in SNMPHosts)
                {
                    WriteVerbose("Removing " + hostExist.PermittedHost + " from list of hosts to be added...");
                    validHostsQuery = validHostsQuery.Where(exist => exist != hostExist.PermittedHost).ToArray();
                }
            }

            if (validHostsQuery.Count() == 0)
                throw new Exception("Submitted host list is empty!");

            // Execute
            WriteVerbose("Adding " + validHostsQuery.Count() + " hosts...");
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                RemoteAddSNMPHosts(validHostsQuery, Computer, Credential);

                WriteVerbose("Retrieving list of current SNMP Hosts from Computer: " + Computer);
                SNMPHosts = RemoteGetSNMPHosts(Computer, Credential);
            }
            else
            {
                AddSNMPHosts(validHostsQuery);

                WriteVerbose("Retrieving list of current SNMP Hosts...");
                SNMPHosts = GetSNMPHosts();
            }

            // Confirm
            LowerHost = Array.ConvertAll(validHostsQuery, host => host.ToLower());

            SNMPHosts = SNMPHosts.Where(host => LowerHost.Contains(host.PermittedHost.ToLower()));

            if (SNMPHosts.Count() == validHostsQuery.Count())
            {
                WriteObject("Successfully added all valid Permitted Hosts");
            }
            else if (SNMPHosts.Count() > 0)
            {
                string[] hostArr = SNMPHosts.Select(host => host.PermittedHost.ToLower()).ToArray();
                foreach (string host in validHostsQuery)
                {
                    bool Match = hostArr.Contains(host);
                    if (Match == false)
                    {
                        WriteObject("Failed to add Permitted Host: " + host);
                    }
                }
            }
            else
            {
                throw new Exception("Failed to add specified SNMP Permitted Hosts");
            }

            SNMPHosts.ToList().ForEach(WriteObject);
        }

        private static void AddSNMPHosts(string[] Hosts)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegHosts = Registry.LocalMachine.OpenSubKey(common.RegHosts);
            List<int> valueNames = new List<int>();

            foreach (string valueName in RegHosts.GetValueNames())
            {
                if (valueName != "(Default)")
                {
                    valueNames.Add(Convert.ToInt32(valueName));
                }
            }
            RegHosts.Close();

            RegHosts = Registry.LocalMachine.CreateSubKey(common.RegHosts);
            int val = 1;
            string Value;
            bool valExist = true;
            foreach (var Host in Hosts)
            {
                while (valExist == true)
                {
                    valExist = valueNames.Contains(val);
                    if (valExist == true) { val++; }
                }
                Value = Convert.ToString(val);
                RegHosts.SetValue(Value, Host);
                valueNames.Add(val);
                valExist = true;
                val = 1;
            }
            RegHosts.Close();
        }

        private static void RemoteAddSNMPHosts(string[] Hosts, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            List<int> valueNames = new List<int>();

            ManagementClass mc = SNMPAgentCommon.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] strValueNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            if (strValueNames != null)
            {
                foreach (string ValueName in strValueNames)
                {
                    if (ValueName != "(Default)")
                    {
                        valueNames.Add(Convert.ToInt32(ValueName));
                    }
                }
            }

            mboIn = mc.GetMethodParameters("SetStringValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            int val = 1;
            string Value;
            bool valExist = strValueNames != null;

            foreach (var Host in Hosts)
            {
                while (valExist == true)
                {
                    valExist = valueNames.Contains(val);
                    if (valExist == true) { val++; }
                }
                Value = Convert.ToString(val);
                mboIn["sValueName"] = Value;
                mboIn["sValue"] = Host;
                mc.InvokeMethod("SetStringValue", mboIn, null);
                valExist = true;
                val = 1;
            }

            mboIn.Dispose();
        }
    }
}