---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Get-SNMPProperties

## SYNOPSIS
Get the current SNMP Agent Properties

## SYNTAX

```
Get-SNMPProperties [-Computer <String>] [-Credential <PSCredential>] [<CommonParameters>]
```

## DESCRIPTION
Get the currently configured SNMP Agent Properties

## EXAMPLES

### Example 1
```
PS C:\> Get-SNMPProperties
```

Returns the list of current SNMP Agent Properties

## PARAMETERS

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

### None
## OUTPUTS

### PSSNMPAgent.Common.SNMPProperties
Output Type of SNMPProperties objects.

## NOTES

## RELATED LINKS
