# Source: https://stackoverflow.com/a/62060315
# Generate self-signed certificate to be used by IdentityServer.
# When using localhost - API cannot see the IdentityServer from within the docker-compose'd network.
# You have to run this script as Administrator (open Powershell by right click -> Run as Administrator).

$ErrorActionPreference = "Stop"

$rootCN = "todolist"
$webApiCNs = "todolistapi", "localhost"
$webClientCNs = "todolistblazor", "localhost"

$alreadyExistingCertsRoot = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq "CN=$rootCN"}
$alreadyExistingCertsApi = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $webApiCNs[0])}
$alreadyExistingCertsClient = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object {$_.Subject -eq ("CN={0}" -f $webClientCNs[0])}

if ($alreadyExistingCertsRoot.Count -eq 1) {
    Write-Output "Skipping creating Root CA certificate as it already exists."
    $testRootCA = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsRoot[0]
} else {
    $testRootCA = New-SelfSignedCertificate -Subject $rootCN -KeyUsageProperty Sign -KeyUsage CertSign -CertStoreLocation Cert:\LocalMachine\My
}

if ($alreadyExistingCertsApi.Count -eq 1) {
    Write-Output "Skipping creating API certificate as it already exists."
    $webApiCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsApi[0]
} else {
    # Create a SAN cert for both web-api and localhost.
    $webApiCert = New-SelfSignedCertificate -DnsName $webApiCNs -Signer $testRootCA -CertStoreLocation Cert:\LocalMachine\My
}

if ($alreadyExistingCertsClient.Count -eq 1) {
    Write-Output "Skipping creating Client certificate as it already exists."
    $webClientCert = [Microsoft.CertificateServices.Commands.Certificate] $alreadyExistingCertsClient[0]
} else {
    # Create a SAN cert for both web-client and localhost.
    $webClientCert = New-SelfSignedCertificate -DnsName $webClientCNs -Signer $testRootCA -CertStoreLocation Cert:\LocalMachine\My
}

# Export it for docker container to pick up later.
$password = ConvertTo-SecureString -String "1234" -Force -AsPlainText

$webApiCertPath = "ToDoListApi/Certificates"
$clientApiCertPath = "ToDoListBlazor/Certificates"

[System.IO.Directory]::CreateDirectory($webApiCertPath) | Out-Null
[System.IO.Directory]::CreateDirectory($clientApiCertPath) | Out-Null

Export-PfxCertificate -Cert $testRootCA -FilePath "$webApiCertPath/todolistroot.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $testRootCA -FilePath "$clientApiCertPath/todolistroot.pfx" -Password $password | Out-Null

Export-PfxCertificate -Cert $webApiCert -FilePath "$webApiCertPath/todolistapi.pfx" -Password $password | Out-Null
Export-PfxCertificate -Cert $webClientCert -FilePath "$clientApiCertPath/todolistblazor.pfx" -Password $password | Out-Null

# Export .cer to be converted to .crt to be trusted within the Docker container.
$rootCertPathCer1 = "ToDoListApi/Certificates/todolistroot.cer"
Export-Certificate -Cert $testRootCA -FilePath $rootCertPathCer1 -Type CERT | Out-Null
$rootCertPathCer2 = "ToDoListBlazor/Certificates/todolistroot.cer"
Export-Certificate -Cert $testRootCA -FilePath $rootCertPathCer2 -Type CERT | Out-Null

# Trust it on your host machine.
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store "Root","LocalMachine"
$store.Open("ReadWrite")

$rootCertAlreadyTrusted = ($store.Certificates | Where-Object {$_.Subject -eq "CN=$rootCN"} | Measure-Object).Count -eq 1

if ($rootCertAlreadyTrusted -eq $false) {
    Write-Output "Adding the root CA certificate to the trust store."
    $store.Add($testRootCA)
}

$store.Close
