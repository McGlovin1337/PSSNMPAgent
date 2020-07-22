using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace PSSNMPAgent.Common
{
    public class SNMPCommunity
    {
        public string Community { get; set; }
        public string AccessRight { get; set; }
    }

    public class SNMPHost
    {
        public string PermittedHost { get; set; }
    }

    public class SNMPTrap
    {
        public string Community { get; set; }
        public string Destination { get; set; }
    }

    public class SNMPProperties
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

        public class communityAccess
        {
            public string Access;
            public int dWordVal;
        }

        private List<communityAccess> _communityAccess = new List<communityAccess>
        {
            new communityAccess() { Access = "None", dWordVal = 1 },
            new communityAccess() { Access = "Notify", dWordVal = 2},
            new communityAccess() { Access = "ReadOnly", dWordVal = 4},
            new communityAccess() { Access = "ReadWrite", dWordVal = 8},
            new communityAccess() { Access = "ReadCreate", dWordVal = 16}
        };

        public ReadOnlyCollection<communityAccess> CommunityAccess
        {
            get { return new ReadOnlyCollection<communityAccess>(_communityAccess); }
        }

        public static IEnumerable<SNMPCommunity> GetCommunities()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegCommunities = Registry.LocalMachine.OpenSubKey(common.RegCommunities);

            List<SNMPCommunity> communities = new List<SNMPCommunity>();

            foreach (string Community in RegCommunities.GetValueNames())
            {
                int accessValue = (int)RegCommunities.GetValue(Community);
                var accessType = common.CommunityAccess.Single(a => a.dWordVal == accessValue);
                string access = accessType.Access;
                communities.Add(new SNMPCommunity { Community = Community, AccessRight = access });
            }
            RegCommunities.Close();

            return communities;
        }

        public static IEnumerable<SNMPHost> GetSNMPHosts()
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

        public static IEnumerable<SNMPTrap> GetSNMPTraps()
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            RegistryKey RegTrap = Registry.LocalMachine.OpenSubKey(common.RegTraps);

            List<SNMPTrap> traps = new List<SNMPTrap>();

            foreach (string key in RegTrap.GetSubKeyNames())
            {
                string subkey = common.RegTraps + @"\" + key;
                RegistryKey RegTrapDest = Registry.LocalMachine.OpenSubKey(subkey);
                foreach (string value in RegTrapDest.GetValueNames())
                {
                    string destination = (string)RegTrapDest.GetValue(value);
                    traps.Add(new SNMPTrap { Community = key, Destination = destination });
                }
                RegTrapDest.Close();
            }
            RegTrap.Close();

            return traps;
        }

        public static IEnumerable<SNMPProperties> GetSNMPProperties()
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
    }

    public class SNMPTrapComparer : IEqualityComparer<SNMPTrap>
    {
        public bool Equals(SNMPTrap trap1, SNMPTrap trap2)
        {
            if (trap1.Community == trap2.Community && trap1.Destination == trap2.Destination)
            {
                return true;
            }
            else return false;
        }
        public int GetHashCode(SNMPTrap obj)
        {
            return obj.Destination.GetHashCode();
        }
    }
}