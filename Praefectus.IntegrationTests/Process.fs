module Praefectus.IntegrationTests.Process

open System
open System.IO
open System.Diagnostics

open Praefectus.IntegrationTests
open Xunit

let run(args: string seq): Async<Process> = async {
    let executablePath = Configuration.executableUnderTest
    let extension = Path.GetExtension executablePath
    let shouldUseLauncher = extension = ".dll"

    // Use dotnet launcher if we're supplied with a DLL path; start the process directly otherwise.
    let startInfo =
        if shouldUseLauncher
        then
            let si = ProcessStartInfo "/usr/bin/env"
            si.ArgumentList.Add "dotnet"
            si.ArgumentList.Add executablePath
            si
        else ProcessStartInfo executablePath
    args |> Seq.iter startInfo.ArgumentList.Add
    startInfo.RedirectStandardOutput <- true

    return Process.Start startInfo
}

let assertExitCode (proc: Process) (code: int): Async<unit> = async {
    proc.WaitForExit()
    Assert.Equal(code, proc.ExitCode)
}
