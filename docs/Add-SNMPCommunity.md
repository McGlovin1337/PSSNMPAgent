---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Add-SNMPCommunity

## SYNOPSIS
Add new SNMP Community Names

## SYNTAX

```
Add-SNMPCommunity [-Communities] <String[]> [[-AccessRight] <String>] [<CommonParameters>]
```

## DESCRIPTION
Add new SNMP Community Names and set their Access Rights

## EXAMPLES

### Example 1
```powershell
PS C:\> Add-SNMPCommunity -Communities private -AccessRight ReadCreate

Community     AccessRights
---------     ------------
private		  ReadCreate
```

Adds a new SNMP Community with the Access Right of ReadCreate

## PARAMETERS

### -AccessRight
The Acces Right to assign to new Community Names

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: None, Notify, ReadOnly, ReadWrite, ReadCreate

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Communities
The SNMP Community Names to add

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

### System.String

## OUTPUTS

### PSSNMPAgent.Common.SNMPCommunity
Output type for SNMPCommunity objects

## NOTES

## RELATED LINKS
