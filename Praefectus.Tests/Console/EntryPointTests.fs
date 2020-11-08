module Praefectus.Tests.Console.EntryPointTests

open System.IO
open System.Reflection

open Praefectus.Console
open Xunit

open Praefectus.Console.EntryPoint

exception private ExitCodeException of code: int

let private testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
let private runMain arguments =
    let mutable exitCode = None
    let terminator =
        { new ITerminator with
            member _.Terminate code =
                exitCode <- Some code
                raise <| ExitCodeException code }
    try
        let actualCode = run testDirectory arguments (Some terminator)
        Option.defaultValue actualCode exitCode
    with
    | :? ExitCodeException as ex -> ex.code

[<Fact>]
let ``Console should return code 2 without options``(): unit =
    let exitCode = runMain [||]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)

[<Fact>]
let ``Console should return special code on unknown option``(): unit =
    let exitCode = runMain [| "unknown" |]
    Assert.Equal(ExitCodes.CannotParseArguments, exitCode)
