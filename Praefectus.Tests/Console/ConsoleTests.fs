module Praefectus.Tests.Console.EntryPointTests

open System.IO
open System.Reflection

open Xunit

open Praefectus.Console.EntryPoint

let testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

[<Fact>]
let ``Console should return a success code without options``(): unit =
    let exitCode = run testDirectory [||]
    Assert.Equal(ExitCodes.Success, exitCode)

[<Fact>]
let ``Console should return special code on unknown option``(): unit =
    let exitCode = run testDirectory [| "unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)
