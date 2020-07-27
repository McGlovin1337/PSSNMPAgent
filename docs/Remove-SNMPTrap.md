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
Remove-SNMPTrap [-Community] <String[]> [[-Destination] <String[]>] [-Computer <String>]
 [-Credential <PSCredential>] [<CommonParameters>]
```

## DESCRIPTION
Remove SNMP Trap Community Names and Destination Hosts

## EXAMPLES

### Example 1
```
PS C:\> Remove-SNMPTrap -Community Monitoring -Destination traps.local
```

Removes the host "traps.local" from the Community Name "Monitoring"

### Example 2
```
PS C:\> Remove-SNMPTrap -Community Monitoring
```

Removes the SNMP Trap Community named "Monitoring" including all assigned Destinations

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

### -Computer
Connect to Computer

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Credential
Remote Computer Credentials

```yaml
Type: PSCredential
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
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
