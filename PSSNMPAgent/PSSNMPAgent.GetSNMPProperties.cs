using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSSNMPAgent.SNMPPropertyCmdlets
{
    [Cmdlet(VerbsCommon.Get, nameof(SNMPProperties))]
    [OutputType(typeof(SNMPProperties))]

    public class GetSNMPProperty: BaseSNMPProperties
    {
        protected override void ProcessSNMPProperty(IEnumerable<SNMPProperties> SNMPProperties)
        {
            SNMPProperties.ToList().ForEach(WriteObject);
        }
    }
}