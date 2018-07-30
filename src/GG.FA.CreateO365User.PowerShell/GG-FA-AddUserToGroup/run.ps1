# queue trigger: $triggerInput

$requestBody = Get-Content $triggerInput -Raw | ConvertFrom-Json
$securityGroup = $requestBody.securityGroup
$userPrincipalName = $requestBody.userPrincipalName
$adminUser = $requestBody.adminUser
$adminPassword = $requestBody.password

$savedErrorAction = $global:ErrorActionPreference
$global:ErrorActionPreference = 'Stop'

$result = Get-Variable "EXECUTION_CONTEXT_FUNCTIONDIRECTORY"
$funcDir = $result.Value
. "$funcDir/../Common/Exchange.ps1"
	
$ExoSession = Connect-ExchangeOnline -AdminUser $adminUser -AdminPassword $adminPassword

Write-Output "Connected to Exchange Online"

try {
	Write-Output "Trying to add user $userPrincipalName to the group with ID $securityGroup"
    Add-DistributionGroupMember -Identity $securityGroup -Member $userPrincipalName -ErrorAction Stop
}
catch{
	Write-Output "Adding user $userPrincipalName to the group with ID $securityGroup failed."
	$_
}
finally{	
	Disconnect-ExchangeOnline
	Write-Output "Disconnected from Exchange Online"
	$global:ErrorActionPreference=$savedErrorAction
}
