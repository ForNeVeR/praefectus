<#
    .SYNOPSIS
        The purpose of this script is to extract the version information from the published artifact, and to print it
        to the standard output.

        It is used during CI builds to generate name for the artifact to upload.
    .PARAMETER SourceRoot
        Path to the repository source root.
    .PARAMETER MainExecutable
        Full path to the executable to extract the version information from.
#>
param (
    [string] $SourceRoot = "$PSScriptRoot/..",
    [string] $MainExecutable = "$SourceRoot/publish/praefectus.dll"
)

$file = Get-Item $MainExecutable
$file.VersionInfo.ProductVersion
