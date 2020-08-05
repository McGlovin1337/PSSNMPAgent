---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Add-SNMPTrap

## SYNOPSIS
Add new SNMP Trap Community Names and Destination Hosts

## SYNTAX

```
Add-SNMPTrap [[-Community] <String[]>] [[-Destination] <String[]>] [-Computer <String>]
 [-Credential <PSCredential>] [<CommonParameters>]
```

## DESCRIPTION
Add new SNMP Trap Community Names and Destination Hosts and add new Destination Hosts to existing Community Names

## EXAMPLES

### Example 1
```
PS C:\> Add-SNMPTrap -Community Monitoring -Destination monitor.local, traps.local

Community  Destination
---------  -----------
Monitoring monitor.local
Monitoring traps.local
```

Creates the Community Name "Monitoring" if it doesn't already exist and adds the hosts "monitor.local" and "traps.local" to the list of destinations

## PARAMETERS

### -Community
Add the SNMP Trap Community names

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

### -Destination
Add Trap Destination hosts to all specified Community Names

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
Output Type of SNMPTrap objects.

## NOTES

## RELATED LINKS
