module Praefectus.Tests.Console.ConsoleTestUtils

open System
open System.IO
open System.Reflection

open Praefectus.Console

exception private ExitCodeException of code: int

let private testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

let private runMainWithOutput arguments stdOut =
    let mutable exitCode = None
    let terminator =
        { new ITerminator with
            member _.Terminate code =
                exitCode <- Some code
                raise <| ExitCodeException code }
    let env = {
        Terminator = terminator
        Output = stdOut
    }
    try
        let actualCode = EntryPoint.run testDirectory arguments env
        Option.defaultValue actualCode exitCode
    with
    | :? ExitCodeException as ex -> ex.code

let runMain (arguments: string[]): int =
    use stdOut = Console.OpenStandardOutput()
    runMainWithOutput arguments stdOut

let runMainCapturingOutput (arguments: string[]): int * Stream =
    let stream = new MemoryStream()
    let exitCode = runMainWithOutput arguments stream
    stream.Position <- 0L
    exitCode, upcast stream
