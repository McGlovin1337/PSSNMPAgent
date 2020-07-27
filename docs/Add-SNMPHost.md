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
Add-SNMPHost -PermittedHost <String[]> [-Computer <String>] [-Credential <PSCredential>] [<CommonParameters>]
```

## DESCRIPTION
Add new Hosts permitted to acces the SNMP Service using the configured Community Names.

## EXAMPLES

### Example 1
```
PS C:\> Add-SNMPHost -PermittedHost 127.0.0.1, monitor.local

Host
----
127.0.0.1
monitor.local
```

Add 127.0.0.1 and monitor.local to the list of permitted hosts.

## PARAMETERS

### -PermittedHost
Add a new SNMP Permitted Manager

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: Hosts, Host, Manager, PermittedManager

Required: True
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

### PSSNMPAgent.Common.SNMPHost
Output Type of SNMPHost objects.

## NOTES

## RELATED LINKS
