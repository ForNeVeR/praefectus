param (
    $SolutionRoot = "$PSScriptRoot/..",
    $TestResultDirectories = "$SolutionRoot/Praefectus.Tests/**/TestResults"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Get-ChildItem -Recurse $TestResultDirectories -Filter "*.received.txt" | ForEach-Object {
    $receivedTestResult = $_.FullName
    $approvedTestResult = $receivedTestResult.Replace('.received.txt', '.approved.txt')
    Move-Item -Force -LiteralPath $receivedTestResult $approvedTestResult
}
