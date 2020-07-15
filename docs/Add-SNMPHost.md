---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Add-SNMPHost

## SYNOPSIS
Add new SNMP Permitted Hosts

## SYNTAX

```
Add-SNMPHost [-Hosts] <String[]> [<CommonParameters>]
```

## DESCRIPTION
Add new Hosts permitted to acces the SNMP Service using the configured Community Names.

## EXAMPLES

### Example 1
```powershell
PS C:\> Add-SNMPHost -Hosts 127.0.0.1, monitor.local

Host
----
127.0.0.1
monitor.local
```

Add 127.0.0.1 and monitor.local to the list of permitted hosts.

## PARAMETERS

### -Hosts
Add a new SNMP Permitted Manager

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

## OUTPUTS

### PSSNMPAgent.Common.SNMPHost
Output Type of SNMPHost objects.

## NOTES

## RELATED LINKS
