Param(
    [Parameter(Mandatory = $True)]
    [string]$srcFolder,

    [Parameter(Mandatory = $True)]
    [string]$mainProjectFile
)

. "$PSScriptRoot\Functions.ps1"

$binDir = Join-Path $srcFolder "HttPlaceholder\bin\release\netcoreapp2.2\osx-x64\publish"
$docsFolder = Join-Path $srcFolder "..\docs"

# Create OS X package
Write-Host "Packing up for OS X" -ForegroundColor Green
& dotnet publish $mainProjectFile --configuration=release --runtime=osx-x64
Assert-Cmd-Ok

# Moving docs folder to bin path
Copy-Item $docsFolder (Join-Path $binDir "docs") -Recurse -Container

# Creating .tar.gz file of binaries.
& 7z a -ttar "$binDir\httplaceholder_osx-x64.tar" "$binDir\**"
Assert-Cmd-Ok
& 7z a -tgzip "$binDir\..\..\httplaceholder_osx-x64.tar.gz" "$binDir\httplaceholder_osx-x64.tar"
Assert-Cmd-Ok
Remove-Item "$binDir\httplaceholder_osx-x64.tar"