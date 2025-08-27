param()
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Join-Path $root ".."
cd $repo
dotnet new sln -n ResilienceDemo -o .
dotnet sln add src/TechScriptAid.ResilienceDemo.API/TechScriptAid.ResilienceDemo.API.csproj
dotnet sln add tests/TechScriptAid.ResilienceDemo.Tests/TechScriptAid.ResilienceDemo.Tests.csproj
Write-Host "Solution created: ResilienceDemo.sln"
