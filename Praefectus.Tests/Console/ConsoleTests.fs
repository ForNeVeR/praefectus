module Praefectus.Tests.Console.EntryPointTests

open Xunit

open Praefectus.Console.EntryPoint

[<Fact>]
let ``Console should return a success code without options``(): unit =
    let exitCode = main [||]
    Assert.Equal(ExitCodes.Success, exitCode)

[<Fact>]
let ``Console should return special code on unknown option``(): unit =
    let exitCode = main [| "unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)
