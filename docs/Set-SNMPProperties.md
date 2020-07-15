---
external help file: PSSNMPAgent.dll-Help.xml
Module Name: PSSNMPAgent
online version:
schema: 2.0.0
---

# Set-SNMPProperties

## SYNOPSIS
Set the SNMP Agent Properties

## SYNTAX

```
Set-SNMPProperties [[-SysContact] <String>] [[-SysLocation] <String>] [-EnableAuthTraps]
 [[-NameResolutionRetries] <Int32>] [-SvcPhysical] [-SvcApplications] [-SvcDatalink] [-SvcInternet]
 [-SvcEndToEnd] [<CommonParameters>]
```

## DESCRIPTION
Set the SNMP Agent Properties and select the Services that are Enabled and Disabled.

## EXAMPLES

### Example 1
```powershell
PS C:\> Set-SNMPProperties -SysContact "Joe Bloggs" -SysLocation "London"
```

Set the Contact Property to "Joe Bloggs" and the Location Property to "London"

### Example 2
```powershell
PS C:\> Set-SNMPProperties -EnableAuthTraps:$false -SvcPhysical
```

Disables Authentication Traps and Enables the Physical Service.

## PARAMETERS

### -EnableAuthTraps
Enable/Disable Authentication Traps

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NameResolutionRetries
Number of Name Resolution Retries

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SvcApplications
Enable/Disable Service: Applications

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 5
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SvcDatalink
Enable/Disable Service: Datalink and Subnetwork

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 6
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SvcEndToEnd
Enable/Disable Service: End-to-End

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 8
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SvcInternet
Enable/Disable Service: Internet

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 7
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SvcPhysical
Enable/Disable Service: Physical

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SysContact
Set System Contact Details

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SysLocation
Set System Location Details

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

### System.Int32

## OUTPUTS

### PSSNMPAgent.Common.SNMPProperties
Output Type of SNMPProperties.

## NOTES

## RELATED LINKS
