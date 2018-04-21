#Requires -Version 3.0

Param(
		[string]$WebUrl,
		[string]$UserName,
		[string]$Password,
		[string]$XmlPnPFile = "PnPXml/UserInventoryRessources.xml"
)

$securePassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
$credentials = New-Object -typename System.Management.Automation.PSCredential -argumentlist $UserName, $securePassword

Connect-PnPOnline -Url $WebUrl -Credentials $credentials


$XmlPnPFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $XmlPnPFile))

Apply-PnPProvisioningTemplate -Path $XmlPnPFile 