Function Connect-ExchangeOnline{
	Param(
		[Parameter(Mandatory = $true)]
        [string]$AdminUser,
        [Parameter(Mandatory = $true)]
        [string]$AdminPassword
	)
	$securePass = $(convertto-securestring $AdminPassword -asplaintext -force)
	$credentials = New-Object  System.Management.Automation.PSCredential($AdminUser, $securePass) 

	$session = New-PSSession -ConfigurationName Microsoft.Exchange -ConnectionUri https://outlook.office365.com/powershell-liveid/ -Credential $credentials -Authentication Basic -AllowRedirection
	$null = Import-PSSession $session -DisableNameChecking -AllowClobber -ErrorAction SilentlyContinue   

	$erroractionpreference = 'stop' 

	return $session
}


Function Disconnect-ExchangeOnline{
        $erroractionpreference = 'continue'
        Get-PSSession | Remove-PSSession
}