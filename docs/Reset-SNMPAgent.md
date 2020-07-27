---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Reset-SNMPAgent

## SYNOPSIS
Reset SNMP Agent to Installation Defaults

## SYNTAX

```
Reset-SNMPAgent [-Computer <String>] [-Credential <PSCredential>] [<CommonParameters>]
```

## DESCRIPTION
Resets the Windows SNMP Agent to Installation Default settings.

## EXAMPLES

### Example 1
```
PS C:\> Reset-SNMPAgent
```

Resets the Windows SNMP Agent to Installation Default settings.

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

### System.Object
## NOTES

## RELATED LINKS
