#Requires -Version 3.0

Param(
		[string]$WebUrl,
		[string]$UserName,
		[string]$Password,
		[string]$XmlPnPPath = "PnPXml"
)

$securePassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
$credentials = New-Object -typename System.Management.Automation.PSCredential -argumentlist $UserName, $securePassword

Connect-PnPOnline -Url $WebUrl -Credentials $credentials

$pnpTemplates = ($XmlPnPPath | Get-ChildItem | where {$_.Name -like "*.xml"} | select FullName).FullName

$pnpTemplates | foreach {

	$XmlPnPFile = $_

	Apply-PnPProvisioningTemplate -Path $XmlPnPFile 

}



