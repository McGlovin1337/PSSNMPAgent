---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Get-SNMPHost

## SYNOPSIS
Get configured SNMP Hosts

## SYNTAX

```
Get-SNMPHost [[-Hosts] <String[]>] [<CommonParameters>]
```

## DESCRIPTION
Get the configured SNMP Permitted Hosts/Managers

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-SNMPHost

Host
----
localhost
```

Returns all the configured SNMP Permiteed Hosts

## PARAMETERS

### -Hosts
List matching Hosts

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
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
Output Type for SNMPHost objects

## NOTES

## RELATED LINKS
