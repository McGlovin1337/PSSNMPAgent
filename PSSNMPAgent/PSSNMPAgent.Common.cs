using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Management.Automation;
using System.Management;

namespace PSSNMPAgent.Common
{
    public class SNMPAgentCommon
    {
        private const string regRootSubKey = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters";
        private const string regCommunities = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\ValidCommunities";
        private const string regHosts = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\PermittedManagers";
        private const string regTraps = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\TrapConfiguration";
        private const string regRFC1156 = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\RFC1156Agent";

        public string RegRootSubKey
        {
            get { return regRootSubKey; }
        }

        public string RegCommunities
        {
            get { return regCommunities; }
        }

        public string RegHosts
        {
            get { return regHosts; }
        }

        public string RegTraps
        {
            get { return regTraps; }
        }

        public string RegRFC1156
        {
            get { return regRFC1156; }
        }

        public static void ServiceCheck()
        {
            var Services = ServiceController.GetServices();
            int SvcMatch = 0;
            foreach (var Service in Services)
            {
                if (Service.ServiceName == "SNMP")
                {
                    SvcMatch++;
                    break;
                }
            }

            if (SvcMatch == 0)
            {
                throw new Exception("SNMP Service is Not Installed");
            }
        }

        public static ManagementClass RemoteConnect(string Computer, PSCredential Credential)
        {
            ConnectionOptions conOpt = new ConnectionOptions();
            conOpt.EnablePrivileges = true;
            conOpt.Authentication = AuthenticationLevel.Packet;
            conOpt.Impersonation = ImpersonationLevel.Impersonate;
            if (Credential != null)
            {
                conOpt.Username = Credential.UserName;
                conOpt.SecurePassword = Credential.Password;
            }

            ManagementScope scope = new ManagementScope(@"\\" + Computer + @"\root\default", conOpt);
            scope.Connect();

            ManagementClass mc = new ManagementClass(scope, new ManagementPath("StdRegProv"), null);

            return mc;
        }
    }

    public class ValidateHostname
    {
        public string ValidHostnames { get; set; }
        public string InvalidHostnames { get; set; }

        public static IEnumerable<ValidateHostname> ValidateHostnames(string[] Hostnames)
        {
            List<ValidateHostname> validatedHostnames = new List<ValidateHostname>();
            bool ValidateResult;

            foreach (string Hostname in Hostnames)
            {
                ValidateResult = ValidateHost(Hostname);
                if (ValidateResult == true)
                    validatedHostnames.Add(new ValidateHostname { ValidHostnames = Hostname });
                else
                    validatedHostnames.Add(new ValidateHostname { InvalidHostnames = Hostname });
            }

            return validatedHostnames;
        }

        public static bool ValidateHost(string Hostname)
        {
            var Validated = Regex.Match(Hostname, @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
            if (Validated.Success) return true;
            return false;
        }
    }
}