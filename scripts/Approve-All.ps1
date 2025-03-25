# SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: MIT

param (
    $SolutionRoot = "$PSScriptRoot/..",
    $TestResults = "$SolutionRoot/Praefectus.Tests"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Get-ChildItem -Recurse $TestResults -Filter "*.received.txt" | ForEach-Object {
    $receivedTestResult = $_.FullName
    $approvedTestResult = $receivedTestResult.Replace('.received.txt', '.verified.txt')
    Move-Item -Force -LiteralPath $receivedTestResult $approvedTestResult
}
