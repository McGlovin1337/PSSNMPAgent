# PSSNMPAgent

## About
Powershell Module to Manage Windows SNMP Agent Service

This Powershell Module provides the following Cmdlets:

Add-SNMPCommunity - Adds a new SNMP Community\
Add-SNMPHost - Adds a new SNMP permitted Host/Manager\
Add-SNMPTrap - Adds a new SNMP Trap Community and Destination, or adds a Destination to an existing Community Name\
Get-SNMPCommunity - Retrieves the current SNMP Community Names\
Get-SNMPHost - Retrieves the current permitted SNMP Host/Managers \
Get-SNMPProperties - Retrieves the current SNMP Service Properties\
Get-SNMPTrap - Retrieves the curren SNMP Trap Communities and their configured Destinations\
Remove-SNMPCommunity - Removes SNMP Community Names\
Remove-SNMPHost - Removes SNMP permitted Hosts/Managers\
Remove-SNMPTrap - Removes SNMP Trap Communities and/or Destinations\
Reset-SNMPAgent - Resets the SNMP Agent back to installation default settings\
Set-SNMPProperties - Sets the SNMP Properties

## Install
The PSSNMPAgent Module is compatible with Powershell 4.0+

Install the module from the PowerShell Gallery:

```powershell
Install-Module -Name PSSNMPAgent
```

## Usage
Simply Import the Module to begin using:
```powershell
Import-Module PSSNMPAgent
```
The module includes help for each Cmdlet with Examples, simply use Get-Help, e.g.
```powershell
Get-Help Get-SNMPCommunity -Full
```
