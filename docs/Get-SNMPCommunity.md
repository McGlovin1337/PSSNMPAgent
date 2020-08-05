---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Get-SNMPCommunity

## SYNOPSIS
Get SNMP Agent Communities and Allowed Hosts

## SYNTAX

```
Get-SNMPCommunity [-AccessRight <String[]>] [-Community <String[]>] [-Computer <String>]
 [-Credential <PSCredential>] [<CommonParameters>]
```

## DESCRIPTION
Use Get-SNMPCommunity to return the configured SNMP Community Names and the configured Access Rights.

## EXAMPLES

### EXAMPLE 1
```
PS C:\>Get-SNMPCommunity

Community     AccessRights
---------     ------------
freqa         ReadOnly
iddqd         ReadCreate
```

Get All SNMP Community Names and Access Rights

## PARAMETERS

### -AccessRight
List SNMP Communities by Access Right

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Community
List matching SNMP Communities

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
OutputType for SNMPCommunity objects.

## NOTES

## RELATED LINKS
