using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PSSNMPAgent.SNMPTrapCmdlets
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPTrap))]
    [OutputType(typeof(SNMPTrap))]
    public class GetSNMPTrap: BaseSNMPTrap
    {
        protected override void ProcessSNMPTrap(IEnumerable<SNMPTrap> SNMPTraps, IEnumerable<string> DestinationQuery)
        {
            if (SNMPTraps.Count() > 0)
            {
                if (Community != null)
                {
                    SNMPTraps = SNMPTraps.Where(community => Community.Contains(community.Community));
                }

                if (DestinationQuery != null)
                {
                    SNMPTraps = FilterDestinations(SNMPTraps, DestinationQuery);
                }
            }
            else
            {
                WriteVerbose("No SNMP Trap Communities or Destinations configured.");
            }

            SNMPTraps.ToList().ForEach(WriteObject);
        }

        private static IEnumerable<SNMPTrap> FilterDestinations(IEnumerable<SNMPTrap> Results, IEnumerable<string> Destinations)
        {
            List<SNMPTrap> filteredResults = new List<SNMPTrap>();
            List<string> filteredDestinations = new List<string>();

            foreach (var result in Results)
            {
                foreach (string destination in result.Destination)
                {
                    var Match = Regex.Match(destination, string.Format("^(?i:{0})", string.Join("|", Destinations)));
                    if (Match.Success)
                        filteredDestinations.Add(destination);
                }
                if (filteredDestinations.Count() > 0)
                    filteredResults.Add(new SNMPTrap { Community = result.Community, Destination = filteredDestinations.ToArray() });

                filteredDestinations.Clear();
            }

            return filteredResults;
        }
    }
}