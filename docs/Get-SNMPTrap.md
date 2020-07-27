---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Get-SNMPTrap

## SYNOPSIS
Get the SNMP Trap Community Names and Destination hosts.

## SYNTAX

```
Get-SNMPTrap [[-Community] <String[]>] [[-Destination] <String[]>] [-Computer <String>]
 [-Credential <PSCredential>] [<CommonParameters>]
```

## DESCRIPTION
Get the configured SNMP Trap Community Names and Destination hosts.

## EXAMPLES

### Example 1
```
PS C:\> Get-SNMPTrap

Community Destination
--------- -----------
Monitor   trapshost.local
Service   127.0.0.1
```

Returns all the SNMP Trap Community Names and the Destination Hosts configured for each.

## PARAMETERS

### -Community
Specify Community Names to match

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
Specify Destinations to match

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
