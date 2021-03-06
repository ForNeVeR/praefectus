module Praefectus.Tests.Console.EntryPointTests

open System.IO

open Xunit

open Praefectus.Console
open Praefectus.Console.EntryPoint
open Praefectus.Tests.Console.ConsoleTestUtils

let private testConfiguration = {
    DatabaseLocation = Path.GetTempPath()
    Ordering = Array.empty
}

[<Fact>]
let ``Console should return code 2 without options``(): unit =
    let exitCode = runMain testConfiguration [||]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)

[<Fact>]
let ``Console should return special code on unknown option``(): unit =
    let exitCode = runMain testConfiguration [| "unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)

let ``Exit code should be zero if help was explicitly requested``(): unit =
    let exitCode = runMain testConfiguration [| "--help" |]
    Assert.Equal(ExitCodes.Success, exitCode)

[<Fact>]
let ``Exit code should be zero if subcommand help was explicitly requested``(): unit =
    let exitCode = runMain testConfiguration [| "list"; "--help" |]
    Assert.Equal(ExitCodes.Success, exitCode)

[<Fact>]
let ``Exit code signal a subcommand parse error``(): unit =
    let exitCode = runMain testConfiguration [| "list"; "--unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)
