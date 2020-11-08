module Praefectus.IntegrationTests.ArgumentTests

open System
open System.IO

open Xunit

open Praefectus.Console

[<Fact>]
let ``Version should be reported properly``(): unit =
    async {
        let expectedVersion = EntryPoint.getAppVersion()
        use! exe = Process.run [| "--version" |]
        use output = exe.StandardOutput
        let mutable versionFound = false
        while not output.EndOfStream do
            let! line = Async.AwaitTask <| output.ReadLineAsync()
            if line.StartsWith "Praefectus v"
            then
                let version =
                    line.Split("Praefectus v", StringSplitOptions.RemoveEmptyEntries).[0]
                    |> Version.Parse
                Assert.Equal(expectedVersion, version)
                versionFound <- true
        Assert.True versionFound
    } |> Async.RunSynchronously

[<Fact>]
let ``Exit code should be 2 by default``(): unit =
    async {
        let! exe = Process.run Array.empty
        do! Process.assertExitCode exe EntryPoint.ExitCodes.CannotParseArguments
    } |> Async.RunSynchronously

[<Fact>]
let ``Exit code should be zero if help was explicitly requested``(): unit =
    async {
        let! exe = Process.run [| "--help" |]
        do! Process.assertExitCode exe 0
    } |> Async.RunSynchronously

[<Fact>]
let ``Exit code should be zero if subcommand help was explicitly requested``(): unit =
    async {
        let! exe = Process.run [| "list"; "--help" |]
        do! Process.assertExitCode exe 0
    } |> Async.RunSynchronously

[<Fact>]
let ``Exit code signal a subcommand parse error``(): unit =
    async {
        let! exe = Process.run [| "list"; "--unknown" |]
        do! Process.assertExitCode exe EntryPoint.ExitCodes.CannotParseArguments
    } |> Async.RunSynchronously

[<Fact>]
let ``Exit code should be signal argument parse error``(): unit =
    async {
        let! exe = Process.run [| "--unused" |]
        do! Process.assertExitCode exe EntryPoint.ExitCodes.CannotParseArguments
    } |> Async.RunSynchronously
