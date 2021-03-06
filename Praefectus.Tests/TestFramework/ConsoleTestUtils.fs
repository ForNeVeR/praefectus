﻿module Praefectus.Tests.Console.ConsoleTestUtils

open System
open System.IO

open Praefectus.Storage.FileSystemStorage
open Serilog

open Praefectus.Console

exception private ExitCodeException of code: int

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

let runMain (configuration: Configuration<FileSystemTaskState>) (arguments: string[]): int =
    use stdOut = Console.OpenStandardOutput()
    runMainWithOutput configuration arguments stdOut

let runMainCapturingOutput (configuration: Configuration<FileSystemTaskState>) (arguments: string[]): int * Stream =
    let stream = new MemoryStream()
    let exitCode = runMainWithOutput configuration arguments stream
    stream.Position <- 0L
    exitCode, upcast stream
