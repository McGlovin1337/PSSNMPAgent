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

    public class SNMPAgent
    {
        public string TrapCommunities { get; set; }
        public string TrapHosts { get; set; }
        public bool EnableAuthTraps { get; set; }
        public int NameResolutionRetries { get; set; }
        public string SysContact { get; set; }
        public string SysLocation { get; set; }
    }

    public class SNMPAgentCommon
    {
        private string regRootSubKey = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters";
        private string regCommunities = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\ValidCommunities";
        private string regHosts = @"SYSTEM\CurrentControlSet\Services\SNMP\Parameters\PermittedManagers";

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
