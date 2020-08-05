---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Remove-SNMPCommunity

## SYNOPSIS
Remove SNMP Community Names

## SYNTAX

```
Remove-SNMPCommunity [-Community <String[]>] [-Computer <String>] [-Credential <PSCredential>]
 [<CommonParameters>]
```

## DESCRIPTION
Remove SNMP Community Names

## EXAMPLES

### Example 1
```
PS C:\> Remove-SNMPCommunity -Community Monitoring
```

Removes the SNMP Community Name "Monitoring"

## PARAMETERS

### -Community
Remove the specified SNMP Community Names

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

### PSSNMPAgent.Common.SNMPCommunity
Output Type of SNMPCommunity objects.

## NOTES

## RELATED LINKS
