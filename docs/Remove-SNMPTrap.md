---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Remove-SNMPTrap

## SYNOPSIS
Remove SNMP Trap Community Names and Destination Hosts

## SYNTAX

```
Remove-SNMPTrap [-Community] <String[]> [[-Destination] <String[]>] [-DeleteCommunity] [<CommonParameters>]
```

## DESCRIPTION
Remove SNMP Trap Community Names and Destination Hosts

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-SNMPTrap -Community Monitoring -Destination traps.local 
```

Removes the host "traps.local" from the Community Name "Monitoring"

## PARAMETERS

### -Community
Specify Community Names

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

### -DeleteCommunity
Delete the specified Community Name including all Trap Destinations

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Destination
Specify SNMP Trap Destinations to remove from specified Comunity Names

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

## OUTPUTS

### PSSNMPAgent.Common.SNMPTrap

## NOTES

## RELATED LINKS
