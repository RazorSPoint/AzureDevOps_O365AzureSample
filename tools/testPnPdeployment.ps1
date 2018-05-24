$secpasswd = ConvertTo-SecureString "***REMOVED***" -AsPlainText -Force
$adminCredentials = New-Object System.Management.Automation.PSCredential("sebastian.schuetze@***REMOVED***", $secpasswd)

Connect-PnPOnline -Url ***REMOVED*** -Credentials $adminCredentials
$site = New-PnPTenantSite -Title UserAdminTest -Url "/sites/UserAdministrationTest" -Owner sebastian.schuetze@***REMOVED*** -Template "STS#0" -TimeZone 4 -Force -Wait

Set-PnPTraceLog -On -Level Debug
Connect-PnPOnline -Url "***REMOVED***/sites/UserAdministrationTest" -Credentials $adminCredentials
Apply-PnPProvisioningTemplate -Path ".\01_WebSettings.xml"
Apply-PnPProvisioningTemplate -Path ".\02_ListsFieldsNContentTypes.xml" 

Remove-PnPTenantSite -Url "/sites/UserAdministrationTest" -Force
Clear-PnPTenantRecycleBinItem -Url "***REMOVED***/sites/UserAdministrationTest" -Force