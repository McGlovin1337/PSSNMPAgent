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

                if (Values != null)
                {
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

        public static void RemoteAddSNMPCommunity(string[] Communities, string Access, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            int AccessValue = common.CommunityAccess.Single(val => val.Access == Access).dWordVal;

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("SetDWORDValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            foreach (string Community in Communities)
            {
                mboIn["sValueName"] = Community;
                mboIn["uValue"] = AccessValue;

                mc.InvokeMethod("SetDWORDValue", mboIn, null);
            }

            mboIn.Dispose();
        }

        public static void RemoteAddSNMPHosts(string[] Hosts, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();
            List<int> valueNames = new List<int>();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] strValueNames = (string[])mboOut["sNames"];

            mboIn.Dispose();
            mboOut.Dispose();

            foreach (string ValueName in strValueNames)
            {
                if (ValueName != "(Default)")
                {
                    valueNames.Add(Convert.ToInt32(ValueName));
                }
            }

            ManagementBaseObject mboIn2 = mc.GetMethodParameters("SetStringValue");
            mboIn2["hDefKey"] = (UInt32)2147483650;
            mboIn2["sSubKeyName"] = common.RegHosts;

            int val = 1;
            String Value;
            bool valExist = true;

            foreach (var Host in Hosts)
            {
                while (valExist == true)
                {
                    valExist = valueNames.Contains(val);
                    if (valExist == true) { val++; }
                }
                Value = Convert.ToString(val);
                mboIn2["sValueName"] = Value;
                mboIn2["sValue"] = Host;
                mc.InvokeMethod("SetStringValue", mboIn2, null);
                valExist = true;
                val = 1;
            }

            mboIn2.Dispose();
        }

        public static void RemoteAddSNMPTrap(IEnumerable<SNMPTrap> newTraps, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumKey");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegTraps;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumKey", mboIn, null);
            string[] trapKeys = (string[])mboOut["sNames"];
            IEnumerable<string> TrapKeys = trapKeys.ToList();

            mboIn.Dispose();
            mboOut.Dispose();

            IEnumerable<string> Communities = newTraps.Select(x => x.Community).Distinct();
            IEnumerable<string> Destinations = newTraps.Select(y => y.Destination).Distinct();
            TrapKeys = TrapKeys.Where(key => Communities.Contains(key));

            ManagementBaseObject mboIn2 = mc.GetMethodParameters("EnumValues");
            mboIn2["hDefKey"] = (UInt32)2147483650;

            ManagementBaseObject mboIn3 = mc.GetMethodParameters("CreateKey");
            mboIn3["hDefKey"] = (UInt32)2147483650;

            ManagementBaseObject mboIn4 = mc.GetMethodParameters("SetStringValue");
            mboIn4["hDefKey"] = (UInt32)2147483650;

            foreach (string Community in Communities)
            {
                mboIn2["sSubKeyName"] = common.RegTraps + @"\" + Community;
                mboIn3["sSubKeyName"] = common.RegTraps + @"\" + Community;
                mboIn4["sSubKeyName"] = common.RegTraps + @"\" + Community;
                int valueStart = 1;
                bool KeyExist = TrapKeys.Contains(Community);
                if (KeyExist == true)
                {
                    ManagementBaseObject mboOut2 = mc.InvokeMethod("EnumValues", mboIn2, null);
                    string[] sNames = (string[])mboOut2["sNames"];
                    List<string> values = sNames.ToList();
                    values.RemoveAll(x => x == "(Default)");
                    IEnumerable<int> Values = values.Select(x => int.Parse(x)).ToList();
                    valueStart = Values.Max();
                    valueStart++;
                    mboOut2.Dispose();
                }
                else
                {
                    mc.InvokeMethod("CreateKey", mboIn3, null);
                }    

                foreach (string Destination in Destinations)
                {
                    mboIn4["sValueName"] = valueStart.ToString();
                    mboIn4["sValue"] = Destination;

                    mc.InvokeMethod("SetStringValue", mboIn4, null);
                    valueStart++;
                }
            }

            mboIn2.Dispose();
            mboIn3.Dispose();
            mboIn4.Dispose();
        }

        public static void RemoteRemoveSNMPCommunities(IEnumerable<SNMPCommunity> Communities, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("DeleteValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            foreach (var value in Communities)
            {
                if (value.Community != "(Default)")
                {
                    mboIn["sValueName"] = value.Community;

                    mc.InvokeMethod("DeleteValue", mboIn, null);
                }
            }
            mboIn.Dispose();
        }

        public static void RemoteRemoveSNMPHosts(IEnumerable<SNMPHost> Hosts, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = RemoteConnect(Computer, Credential);

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

            ManagementBaseObject mboIn2 = mc.GetMethodParameters("GetStringValue");
            mboIn2["hDefKey"] = (UInt32)2147483650;
            mboIn2["sSubKeyName"] = common.RegHosts;

            foreach (string valueName in ValueNames)
            {
                if (valueName != "(Default)")
                {
                    mboIn2["sValueName"] = valueName;

                    ManagementBaseObject mboOut2 = mc.InvokeMethod("GetStringValue", mboIn2, null);
                    hostname = (string)mboOut2["sValue"];
                    mboOut2.Dispose();

                    if (LowerHosts.Contains(hostname.ToLower()))
                    {
                        valueNames.Add(valueName);
                    }
                }
            }
            mboIn2.Dispose();

            ManagementBaseObject mboIn3 = mc.GetMethodParameters("DeleteValue");
            mboIn3["hDefKey"] = (UInt32)2147483650;
            mboIn3["sSubKeyName"] = common.RegHosts;

            foreach (string value in valueNames)
            {
                mboIn3["sValueName"] = value;

                mc.InvokeMethod("DeleteValue", mboIn3, null);                
            }
            mboIn3.Dispose();
        }

        public static void RemoteDelCommunity(string[] Community, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("DeleteKey");
            mboIn["hDefKey"] = (UInt32)2147483650;
            
            foreach (string community in Community)
            {
                mboIn["sSubKeyName"] = common.RegTraps + @"\" + community;

                mc.InvokeMethod("DeleteKey", mboIn, null);
            }
            mboIn.Dispose();
        }

        public static void RemoteDelTraps(IEnumerable<SNMPTrap> delTraps, string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = RemoteConnect(Computer, Credential);

            string[] subKeys = delTraps.Select(x => x.Community).Distinct().ToArray();
            List<RegTrapValueMap> values = new List<RegTrapValueMap>();

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;

            foreach (var key in subKeys)
            {
                mboIn["sSubKeyName"] = common.RegTraps + @"\" + key;

                ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
                string[] valueNames = (string[])mboOut["sNames"];
                mboOut.Dispose();

                ManagementBaseObject mboIn2 = mc.GetMethodParameters("GetStringValue");
                mboIn2["hDefKey"] = (UInt32)2147483650;
                mboIn2["sSubKeyName"] = common.RegTraps + @"\" + key;

                foreach (string valueName in valueNames)
                {
                    mboIn2["sValueName"] = valueName;

                    ManagementBaseObject mboOut2 = mc.InvokeMethod("GetStringValue", mboIn2, null);
                    string value = (string)mboOut2["sValue"];
                    mboOut2.Dispose();

                    values.Add(new RegTrapValueMap { SubKey = key, ValueName = valueName, Value = value });
                }
                mboIn2.Dispose();
            }
            mboIn.Dispose();

            ManagementBaseObject mboIn3 = mc.GetMethodParameters("DeleteValue");
            mboIn3["hDefKey"] = (UInt32)2147483650;
            foreach (var trap in delTraps)
            {
                mboIn3["sSubKeyName"] = common.RegTraps + @"\" + trap.Community;
                var delTarget = values.Single(value => value.SubKey == trap.Community && value.Value == trap.Destination);
                mboIn3["sValueName"] = delTarget.ValueName;

                mc.InvokeMethod("DeleteValue", mboIn3, null);
            }
            mboIn3.Dispose();
        }

        public static void RemoteResetSNMPAgent(string Computer, PSCredential Credential)
        {
            SNMPAgentCommon common = new SNMPAgentCommon();

            ManagementClass mc = SNMPRemote.RemoteConnect(Computer, Credential);

            ManagementBaseObject mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            ManagementBaseObject mboOut = mc.InvokeMethod("EnumValues", mboIn, null);
            string[] valueNames = (string[])mboOut["sNames"];
            mboIn.Dispose();
            mboOut.Dispose();

            mboIn = mc.GetMethodParameters("DeleteValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            foreach (string value in valueNames)
            {
                mboIn["sValueName"] = value;
                if (value != "(Default)") mc.InvokeMethod("DeleteValue", mboIn, null);
            }
            mboIn.Dispose();
            Array.Clear(valueNames, 0, valueNames.Length);

            mboIn = mc.GetMethodParameters("EnumValues");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;

            mboOut = mc.InvokeMethod("EnumValues", mboIn, null);            
            valueNames = (string[])mboOut["sNames"];
            mboIn.Dispose();
            mboOut.Dispose();

            mboIn = mc.GetMethodParameters("DeleteValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegCommunities;

            foreach (string value in valueNames)
            {
                mboIn["sValueName"] = value;
                if (value != "(Default)") mc.InvokeMethod("DeleteValue", mboIn, null);
            }
            mboIn.Dispose();
            Array.Clear(valueNames, 0, valueNames.Length);

            mboIn = mc.GetMethodParameters("SetStringValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegHosts;
            mboIn["sValueName"] = "1";
            mboIn["sValue"] = "localhost";

            mc.InvokeMethod("SetStringValue", mboIn, null);
            mboIn.Dispose();

            mboIn = mc.GetMethodParameters("EnumKey");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegTraps;

            mboOut = mc.InvokeMethod("EnumKey", mboIn, null);
            valueNames = (string[])mboOut["sNames"];
            mboIn.Dispose();
            mboOut.Dispose();

            mboIn = mc.GetMethodParameters("DeleteKey");
            mboIn["hDefKey"] = (UInt32)2147483650;

            foreach (string valueName in valueNames)
            {
                mboIn["sSubKeyName"] = common.RegTraps + @"\" + valueName;
                mc.InvokeMethod("DeleteKey", mboIn, null);
            }
            mboIn.Dispose();

            mboIn = mc.GetMethodParameters("SetStringValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegRFC1156;
            mboIn["sValueName"] = "sysContact";
            mboIn["sValue"] = "";
            mc.InvokeMethod("SetStringValue", mboIn, null);

            mboIn["sValueName"] = "sysLocation";
            mboIn["sValue"] = "";
            mc.InvokeMethod("SetStringValue", mboIn, null);
            mboIn.Dispose();

            mboIn = mc.GetMethodParameters("SetDWORDValue");
            mboIn["hDefKey"] = (UInt32)2147483650;
            mboIn["sSubKeyName"] = common.RegRFC1156;
            mboIn["sValueName"] = "sysServices";
            mboIn["uValue"] = 76;
            mc.InvokeMethod("SetDWORDValue", mboIn, null);

            mboIn["sSubKeyName"] = common.RegRootSubKey;
            mboIn["sValueName"] = "EnableAuthenticationTraps";
            mboIn["uValue"] = 1;
            mc.InvokeMethod("SetDWORDValue", mboIn, null);

            mboIn["sValueName"] = "NameResolutionRetries";
            mboIn["uValue"] = 16;
            mc.InvokeMethod("SetDWORDValue", mboIn, null);

            mboIn.Dispose();
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