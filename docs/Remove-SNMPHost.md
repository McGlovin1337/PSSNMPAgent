---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Remove-SNMPHost

## SYNOPSIS
Remove SNMP Permitted Hosts

## SYNTAX

### Hosts
```
Remove-SNMPHost [-Hosts] <String[]> [<CommonParameters>]
```

### RemoveAll
```
Remove-SNMPHost [-RemoveAllHosts] [<CommonParameters>]
```

## DESCRIPTION
Remove SNMP Permitted Hosts/Managers

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-SNMPHost -Hosts monitoring.local
```

Remove the host "monitoring.local" from the list of permitted hosts.

## PARAMETERS

### -Hosts
Remove SNMP Permitted Managers

```yaml
Type: String[]
Parameter Sets: Hosts
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -RemoveAllHosts
Remove all SNMP Permitted Managers

```yaml
Type: SwitchParameter
Parameter Sets: RemoveAll
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

### System.Management.Automation.SwitchParameter

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS