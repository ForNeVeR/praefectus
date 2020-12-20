module Praefectus.Tests.Console.EntryPointTests

open Xunit

open Praefectus.Console.EntryPoint
open Praefectus.Tests.Console.ConsoleTestUtils

[<Fact>]
let ``Console should return code 2 without options``(): unit =
    let exitCode = runMain [||]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)

[<Fact>]
let ``Console should return special code on unknown option``(): unit =
    let exitCode = runMain [| "unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)

let ``Exit code should be zero if help was explicitly requested``(): unit =
    let exitCode = runMain [| "--help" |]
    Assert.Equal(ExitCodes.Success, exitCode)

[<Fact>]
let ``Exit code should be zero if subcommand help was explicitly requested``(): unit =
    let exitCode = runMain [| "list"; "--help" |]
    Assert.Equal(ExitCodes.Success, exitCode)

[<Fact>]
let ``Exit code signal a subcommand parse error``(): unit =
    let exitCode = runMain [| "list"; "--unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)
