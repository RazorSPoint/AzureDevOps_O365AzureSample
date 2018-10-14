$secpasswd = ConvertTo-SecureString "Ss//06.1982!" -AsPlainText -Force
$adminCredentials = New-Object System.Management.Automation.PSCredential("sebastian.schuetze@werteakademie.gut-goedelitz.de", $secpasswd)

Connect-PnPOnline -Url https://gutgoedelitz.sharepoint.com -Credentials $adminCredentials
$site = New-PnPTenantSite -Title UserAdminTest -Url "/sites/UserAdministrationTest" -Owner sebastian.schuetze@werteakademie.gut-goedelitz.de -Template "STS#0" -TimeZone 4 -Force -Wait

Set-PnPTraceLog -On -Level Debug
Connect-PnPOnline -Url "https://gutgoedelitz.sharepoint.com/sites/UserAdministrationTest" -Credentials $adminCredentials
Apply-PnPProvisioningTemplate -Path ".\01_WebSettings.xml"
Apply-PnPProvisioningTemplate -Path ".\02_ListsFieldsNContentTypes.xml" 

Remove-PnPTenantSite -Url "/sites/UserAdministrationTest" -Force
Clear-PnPTenantRecycleBinItem -Url "https://gutgoedelitz.sharepoint.com/sites/UserAdministrationTest" -Force