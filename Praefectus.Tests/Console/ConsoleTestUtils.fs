module Praefectus.Tests.Console.ConsoleTestUtils

open System
open System.IO
open System.Reflection

open Serilog

open Praefectus.Console

exception private ExitCodeException of code: int

let private testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

let private runMainWithOutput configuration arguments stdOut =
    let mutable exitCode = None
    let terminator =
        { new ITerminator with
            member _.Terminate code =
                exitCode <- Some code
                raise <| ExitCodeException code }
    let env = {
        Terminator = terminator
        Output = stdOut
        LoggerConfiguration = LoggerConfiguration()
    }
    try
        let actualCode = EntryPoint.run configuration arguments env
        Option.defaultValue actualCode exitCode
    with
    | :? ExitCodeException as ex -> ex.code

let runMain (arguments: string[]): int =
    use stdOut = Console.OpenStandardOutput()
    let configuration = { DatabaseLocation = testDirectory }
    runMainWithOutput configuration arguments stdOut

let runMainCapturingOutput (configuration: Configuration) (arguments: string[]): int * Stream =
    let stream = new MemoryStream()
    let exitCode = runMainWithOutput configuration arguments stream
    stream.Position <- 0L
    exitCode, upcast stream
