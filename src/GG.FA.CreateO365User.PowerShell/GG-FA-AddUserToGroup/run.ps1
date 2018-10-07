# queue trigger: $triggerInput

$requestBody = Get-Content $triggerInput -Raw | ConvertFrom-Json
$securityGroup = $requestBody.securityGroup
$userPrincipalName = $requestBody.userPrincipalName
$adminUser = APPSETTING_O365AdminUser
$adminPassword = APPSETTING_O365AdminPassword

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

	#Sometimes the user is not added, because it was just created. 
	#We check and then fail for the queue to retry
	Start-Sleep -Seconds 30		
	Write-Output "Checking if user is added to group with ID $securityGroup"

	$user = Get-User -Identity $userPrincipalName
	$members = Get-DistributionGroupMember -Identity $securityGroup
	$userDoesExists = $false

	ForEach ($currentUser in $members) {
		If ($currentUser -like $user) {
			$userDoesExists = $true
		} 
	}

	If ($userDoesExists) {
  		Write-Output "$user exists in the group"
	} Else {
		throw "$user does not exist in the group"
	}
}
catch{
	Write-Output "Adding user $userPrincipalName to the group with ID $securityGroup failed."
	throw $_
}
finally{	
	Disconnect-ExchangeOnline
	Write-Output "Disconnected from Exchange Online"
	$global:ErrorActionPreference=$savedErrorAction
}
