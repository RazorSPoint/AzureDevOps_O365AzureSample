# POST method: $req
$requestBody = Get-Content $req -Raw | ConvertFrom-Json
$securityGroup = $requestBody.securityGroup
$userPrincipalName = $requestBody.userPrincipalName
$adminUser =$requestBody.AdminUser
$adminPassword = $requestBody.Password


$result = Get-Variable "EXECUTION_CONTEXT_FUNCTIONDIRECTORY"
$funcDir = $result.Value
. "$funcDir/../Exchange.ps1"
	


$ExoSession = Connect-ExchangeOnline -AdminUser $adminUser -AdminPassword $adminPassword

Write-Output "Connected to Exchange Online"

$retryCount = 5

 for ($i = 0; $i -lt $RetryCount; $i++) {
    try {
        Add-DistributionGroupMember -Identity $securityGroup -Member $userPrincipalName
        $i = $RetryCount
    }
    catch {
        Write-Output "Adding user $userPrincipalName to the group with ID $securityGroup failed. Retry in 10 seconds..."
        Start-Sleep -Seconds 10
    }           
} 


Disconnect-ExchangeOnline

Write-Output "Disconnected from Exchange Online"
