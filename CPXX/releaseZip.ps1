$Rev=$args[0]
Set-Location $PSScriptRoot
Write-Host $PSScriptRoot
if (!$Rev)
{
  # 截取前三位做版本名
  $Rev = Get-Content CPXX.csproj| Select-String "<AssemblyVersion>(\d\.\d.\d)\.\d+</AssemblyVersion>"
  Write-Host $Rev.Matches.Groups[1]
  $Rev = $Rev.Matches.Groups[1]
}
cd  bin\Debug\net5.0-windows
zip CPXX-v$Rev.zip -r *  -x CPXX.runtimeconfig.dev.json
zip CPXX-v$Rev.zip -j ..\..\..\..\README.md
cd ../../../
if (Test-Path .\CPXX-v$Rev.zip) {Remove-Item .\CPXX-v$Rev.zip}
Move-Item bin\Debug\net5.0-windows\CPXX-v$Rev.zip .\