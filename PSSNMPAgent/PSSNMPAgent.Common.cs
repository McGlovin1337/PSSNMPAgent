using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Collections.ObjectModel;

namespace PSSNMPAgent.Common
{
    public class SNMPCommunity
    {
        public string Community { get; set; }
        public string AccessRights { get; set; }
    }

    public class SNMPHost
    {
        public string Host { get; set; }
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
        private string regRootSubKey = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters";
        private string regCommunities = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\ValidCommunities";
        private string regHosts = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\PermittedManagers";
        private string regTraps = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\TrapConfiguration";
        private string regRFC1156 = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\RFC1156Agent";

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
    }
}
