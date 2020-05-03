module Praefectus.Tests.ConsoleTests

open Xunit

open Praefectus.EntryPoint

[<Fact>]
let ``Console should return a success code without options``(): unit =
    let exitCode = Praefectus.EntryPoint.main [||]
    Assert.Equal(ExitCodes.Success, exitCode)

[<Fact>]
let ``Console should return special code on unknown option``(): unit =
    let exitCode = Praefectus.EntryPoint.main [| "unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)
