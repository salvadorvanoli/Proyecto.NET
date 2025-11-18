# Script para completar el refactor de apps separadas

Write-Host "=== Completando refactor Mobile.Credential y Mobile.AccessPoint ===" -ForegroundColor Cyan

# 1. Copiar archivos Android a Credential
Write-Host "1. Copiando archivos Android HCE a Mobile.Credential..." -ForegroundColor Yellow
$credAndroidServices = "c:\Nadia\.NET\Proyecto.NET\src\Mobile.Credential\Platforms\Android\Services"
if (!(Test-Path $credAndroidServices)) {
    New-Item -ItemType Directory -Path $credAndroidServices -Force | Out-Null
}
Copy-Item "c:\Nadia\.NET\Proyecto.NET\src\Mobile\Platforms\Android\Services\NfcCredentialService.cs" $credAndroidServices -Force
Copy-Item "c:\Nadia\.NET\Proyecto.NET\src\Mobile\Platforms\Android\Services\NfcHostCardEmulationService.cs" $credAndroidServices -Force
Copy-Item "c:\Nadia\.NET\Proyecto.NET\src\Mobile\Platforms\Android\AndroidManifest.xml" "c:\Nadia\.NET\Proyecto.NET\src\Mobile.Credential\Platforms\Android\" -Force

$credXml = "c:\Nadia\.NET\Proyecto.NET\src\Mobile.Credential\Platforms\Android\Resources\xml"
if (!(Test-Path $credXml)) {
    New-Item -ItemType Directory -Path $credXml -Force | Out-Null
}
Copy-Item "c:\Nadia\.NET\Proyecto.NET\src\Mobile\Platforms\Android\Resources\xml\*" $credXml -Force
Write-Host "Archivos Android copiados a Credential" -ForegroundColor Green

# 2. Copiar archivos Android a AccessPoint
Write-Host "2. Copiando archivos Android NFC reader a Mobile.AccessPoint..." -ForegroundColor Yellow
Copy-Item "c:\Nadia\.NET\Proyecto.NET\src\Mobile\Platforms\Android\NfcServiceAndroid.cs" "c:\Nadia\.NET\Proyecto.NET\src\Mobile.AccessPoint\Platforms\Android\" -Force
Copy-Item "c:\Nadia\.NET\Proyecto.NET\src\Mobile\Platforms\Android\AndroidManifest.xml" "c:\Nadia\.NET\Proyecto.NET\src\Mobile.AccessPoint\Platforms\Android\" -Force
Write-Host "Archivos Android copiados a AccessPoint" -ForegroundColor Green

Write-Host "=== Refactor completado ===" -ForegroundColor Cyan
