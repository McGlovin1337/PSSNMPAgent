using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSSNMPAgent.Common;
using Microsoft.Win32;
using PSSNMPAgent.Remote;

namespace SetSNMPProperties.cmd
{
    [Cmdlet(VerbsCommon.Set, nameof(SNMPProperties))]
    [OutputType(typeof(SNMPProperties))]

    public class PSSNMPAgent: PSCmdlet
    {
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, HelpMessage = "Set System Contact Details")]
        [AllowNull, AllowEmptyString]
        public string SysContact { get; set; }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, HelpMessage = "Set System Location Details")]
        [AllowNull, AllowEmptyString]
        public string SysLocation { get; set; }

        [Parameter(Position = 0, HelpMessage = "Enable/Disable Authentication Traps")]
        public SwitchParameter EnableAuthTraps { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Number of Name Resolution Retries")]
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

        [Parameter(Position = 9, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Connect to Computer")]
        [ValidateNotNullOrEmpty]
        public string Computer { get; set; }

        [Parameter(Position = 10, ParameterSetName = "Remote", ValueFromPipelineByPropertyName = true, HelpMessage = "Remote Computer Credentials")]
        [Credential, ValidateNotNullOrEmpty]
        public PSCredential Credential { get; set; }

        private IEnumerable<SNMPProperties> _SNMPProperties;
        private IEnumerable<setSNMPProperties> _setSNMPProperties;

        protected override void BeginProcessing()
        {
            List<setSNMPProperties> newProperties = new List<setSNMPProperties>();

            foreach (var Param in MyInvocation.BoundParameters)
            {
                newProperties.Add(new setSNMPProperties { PropertyName = Param.Key, PropertyValue = Param.Value });
            }

            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                WriteVerbose("Retrieving current SNMP Properties from Computer: " + Computer);
                _SNMPProperties = SNMPRemote.RemoteGetSNMPProperties(Computer, Credential);
            }
            else
            {
                WriteVerbose("Retrieving current SNMP Properties...");
                _SNMPProperties = SNMPAgentCommon.GetSNMPProperties();
            }

            _setSNMPProperties = SanitiseProperties(newProperties);

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Computer"))
            {
                SNMPRemote.RemoteSetSNMPProperties(_setSNMPProperties, Computer, Credential);
                _SNMPProperties = SNMPRemote.RemoteGetSNMPProperties(Computer, Credential);
            }
            else
            {
                SetSNMPProperties(_setSNMPProperties);
                _SNMPProperties = SNMPAgentCommon.GetSNMPProperties();
            }            

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var results = _SNMPProperties;

            results.ToList().ForEach(WriteObject);

            base.EndProcessing();
        }

        private static void SetSNMPProperties(IEnumerable<setSNMPProperties> SetProperties)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            RegistryKey RegRoot = Registry.LocalMachine.CreateSubKey(common.RegRootSubKey);
            RegistryKey RegRFC = Registry.LocalMachine.CreateSubKey(common.RegRFC1156);

            int sysSvcValue = (int)RegRFC.GetValue("sysServices");
            
            foreach (var Property in SetProperties)
            {
                switch(Property.PropertyName)
                {
                    case "sysContact":
                    case "sysLocation":
                        RegRFC.SetValue(Property.PropertyName, Property.PropertyValue);
                        break;
                    case "EnableAuthenticationTraps":
                    case "NameResolutionRetries":
                        RegRoot.SetValue(Property.PropertyName, Property.PropertyValue, RegistryValueKind.DWord);
                        break;
                    case "sysServices":
                        if (Property.PropertyValue != null)
                            sysSvcValue = sysSvcValue + (int)Property.PropertyValue;
                        RegRFC.SetValue(Property.PropertyName, sysSvcValue, RegistryValueKind.DWord);
                        break;
                }
            }

            RegRFC.Close();
            RegRoot.Close();
        }

        private IEnumerable<setSNMPProperties> SanitiseProperties(IEnumerable<setSNMPProperties> inputParameters)
        {
            var currentProperties = _SNMPProperties;

            List<setSNMPProperties> cleanSNMPProperties = new List<setSNMPProperties>();
                        
            foreach (var Parameter in inputParameters)
            {
                if (Parameter.PropertyName == "SysContact" || Parameter.PropertyName == "SysLocation")
                {
                    string lowerPropertyName = char.ToLowerInvariant(Parameter.PropertyName[0]) + Parameter.PropertyName.Substring(1);
                    cleanSNMPProperties.Add(new setSNMPProperties { PropertyName = lowerPropertyName, PropertyValue = Parameter.PropertyValue });
                }

                if (Parameter.PropertyName == "EnableAuthTraps")
                {
                    bool paramVal = Convert.ToBoolean((Parameter.PropertyValue).ToString());
                    cleanSNMPProperties.Add(new setSNMPProperties { PropertyName = "EnableAuthenticationTraps", PropertyValue = paramVal });
                }

                if (Parameter.PropertyName == "NameResolutionRetries")
                {
                    if ((int)Parameter.PropertyValue < 0)
                        throw new Exception("NameResolutionRetries Parameter Out of Range");

                    cleanSNMPProperties.Add(new setSNMPProperties { PropertyName = Parameter.PropertyName, PropertyValue = Parameter.PropertyValue });
                }

                int SvcValue = 0;
                if (Parameter.PropertyName == "SvcPhysical" || Parameter.PropertyName == "SvcApplications" || Parameter.PropertyName == "SvcDatalink" || Parameter.PropertyName == "SvcInternet" || Parameter.PropertyName == "SvcEndToEnd")
                {
                    bool currentValue = false;
                    foreach (var prop in currentProperties)
                    {
                        currentValue = (bool)prop.GetType().GetProperty(Parameter.PropertyName).GetValue(prop);
                    }

                    bool paramValue = Convert.ToBoolean((Parameter.PropertyValue).ToString());
                    if (!paramValue == currentValue)
                    {
                        switch(Parameter.PropertyName)
                        {
                            case "SvcPhysical":
                                SvcValue = (paramValue == true) ? SvcValue + 1 : SvcValue - 1;
                                break;
                            case "SvcApplications":
                                SvcValue = (paramValue == true) ? SvcValue + 64 : SvcValue - 64;
                                break;
                            case "SvcDatalink":
                                SvcValue = (paramValue == true) ? SvcValue + 2 : SvcValue - 2;
                                break;
                            case "SvcInternet":
                                SvcValue = (paramValue == true) ? SvcValue + 4 : SvcValue - 4;
                                break;
                            case "SvcEndToEnd":
                                SvcValue = (paramValue == true) ? SvcValue + 8 : SvcValue - 8;
                                break;
                        }
                    }
                }
                cleanSNMPProperties.Add(new setSNMPProperties { PropertyName = "sysServices", PropertyValue = SvcValue });
            }

            return cleanSNMPProperties;
        }
    }
}