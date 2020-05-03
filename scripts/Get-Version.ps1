param (
    [string] $SourceRoot = "$PSScriptRoot/..",
    [string] $MainExecutable = "$SourceRoot/publish/praefectus.dll"
)

$file = Get-Item $MainExecutable
$file.VersionInfo.ProductVersion
