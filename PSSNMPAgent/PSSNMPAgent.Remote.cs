using System;
using System.Collections.Generic;
using System.Linq;
using PSSNMPAgent.Common;
using System.Management;
using System.Management.Automation;

namespace PSSNMPAgent.Remote
{
    public class SNMPRemote
    {
        public static IEnumerable<SNMPCommunity> RemoteGetSNMPCommunity(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] ValueNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            List<SNMPCommunity> communities = new List<SNMPCommunity>();

            foreach (var value in ValueNames)
            {
                ManagementBaseObject mboIn2 = mc.GetMethodParameters("GetDWORDValue");
                mboIn2["hDefKey"] = (UInt32)2147483650;
                mboIn2["sSubKeyName"] = common.RegCommunities;
                mboIn2["sValueName"] = value;

                ManagementBaseObject mboOut2 = mc.InvokeMethod("GetDWORDValue", mboIn2, null);
                var accessType = common.CommunityAccess.Single(a => a.dWordVal == (UInt32)mboOut2["uValue"]);
                string access = accessType.Access;
                communities.Add(new SNMPCommunity { Community = value, AccessRight = access });

                mboIn2.Dispose();
                mboOut2.Dispose();
            }

            return communities;
        }

        public static IEnumerable<SNMPHost> RemoteGetSNMPHosts(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] ValueNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            List<SNMPHost> hosts = new List<SNMPHost>();

            foreach (var value in ValueNames)
            {
                ManagementBaseObject mboIn2 = mc.GetMethodParameters("GetStringValue");
                mboIn2["hDefKey"] = (UInt32)2147483650;
                mboIn2["sSubKeyName"] = common.RegHosts;
                mboIn2["sValueName"] = value;

                ManagementBaseObject mboOut2 = mc.InvokeMethod("GetStringValue", mboIn2, null);
                hosts.Add(new SNMPHost { PermittedHost = (string)mboOut2["sValue"] });

                mboIn2.Dispose();
                mboOut2.Dispose();
            }

            return hosts;
        }

        public static IEnumerable<SNMPTrap> RemoteGetSNMPTrap(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            List<SNMPTrap> traps = new List<SNMPTrap>();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumKey");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegTraps;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumKey", mboIn, null);
            string[] subKeyNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            foreach (var subKeyName in subKeyNames)
            {
                string subKey = common.RegTraps + @"\" + subKeyName;                
                ManagementBaseObject mboIn2 = mc.GetMethodParameters("EnumValues");
                mboIn2["hDefKey"] = (UInt32)2147483650;
                mboIn2["sSubKeyName"] = subKey;

                ManagementBaseObject mboOut2 = mc.InvokeMethod("EnumValues", mboIn2, null);
                string[] Values = (string[])mboOut2["sNames"];

                mboIn2.Dispose();
                mboOut2.Dispose();

                foreach (var value in Values)
                {
                    ManagementBaseObject mboIn3 = mc.GetMethodParameters("GetStringValue");
                    mboIn3["hDefKey"] = (UInt32)2147483650;
                    mboIn3["sSubKeyName"] = subKey;
                    mboIn3["sValueName"] = value;

                    ManagementBaseObject mboOut3 = mc.InvokeMethod("GetStringValue", mboIn3, null);
                    traps.Add(new SNMPTrap { Community = subKeyName, Destination = (string)mboOut3["sValue"] });

                    mboIn3.Dispose();
                    mboOut3.Dispose();
                }
            }

            return traps;
        }

        public static IEnumerable<SNMPProperties> RemoteGetSNMPProperties(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            List<SNMPProperties> properties = new List<SNMPProperties>();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("GetDWORDValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegRootSubKey;
            mboIn["sValueName"] = "EnableAuthenticationTraps";

            ManagementBaseObject mboIn2 = mc.GetMethodParameters("GetDWORDValue");
            mboIn2["hDefKey"] = (UInt32)2147483650;
            mboIn2["sSubKeyName"] = common.RegRootSubKey;
            mboIn2["sValueName"] = "NameResolutionRetries";

            ManagementBaseObject mboIn3 = mc.GetMethodParameters("GetDWORDValue");
            mboIn3["hDefKey"] = (UInt32)2147483650;
            mboIn3["sSubKeyName"] = common.RegRFC1156;
            mboIn3["sValueName"] = "sysServices";

            ManagementBaseObject mboIn4 = mc.GetMethodParameters("GetStringValue");
            mboIn4["hDefKey"] = (UInt32)2147483650;
            mboIn4["sSubKeyName"] = common.RegRFC1156;
            mboIn4["sValueName"] = "sysContact";

            ManagementBaseObject mboIn5 = mc.GetMethodParameters("GetStringValue");
            mboIn5["hDefKey"] = (UInt32)2147483650;
            mboIn5["sSubKeyName"] = common.RegRFC1156;
            mboIn5["sValueName"] = "sysLocation";

            ManagementBaseObject mboOut = mc.InvokeMethod("GetDWORDValue", mboIn, null);
            ManagementBaseObject mboOut2 = mc.InvokeMethod("GetDWORDValue", mboIn2, null);
            ManagementBaseObject mboOut3 = mc.InvokeMethod("GetDWORDValue", mboIn3, null);
            ManagementBaseObject mboOut4 = mc.InvokeMethod("GetStringValue", mboIn4, null);
            ManagementBaseObject mboOut5 = mc.InvokeMethod("GetStringValue", mboIn5, null);

            bool AuthTraps = Convert.ToBoolean((UInt32)mboOut["uValue"]);            
            int NameRetries = Convert.ToInt32((UInt32)mboOut2["uValue"]);
            string Contact = (string)mboOut4["sValue"];
            string Location = (string)mboOut5["sValue"];
            int SvcValue = Convert.ToInt32((UInt32)mboOut3["uValue"]);

            mboIn.Dispose();
            mboIn2.Dispose();
            mboIn3.Dispose();
            mboIn4.Dispose();
            mboIn5.Dispose();
            mboOut.Dispose();
            mboOut2.Dispose();
            mboOut3.Dispose();
            mboOut4.Dispose();
            mboOut5.Dispose();

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

        private static ManagementClass RemoteConnect(string Computer, PSCredential Credential)
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
}