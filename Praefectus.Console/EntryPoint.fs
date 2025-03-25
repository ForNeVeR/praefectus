module Praefectus.Console.EntryPoint

open System
open System.Reflection
open System.Runtime.CompilerServices

open Argu

open Praefectus.Storage.FileSystemStorage
open Praefectus.Utils

module ExitCodes =
    let Success = 0
    let GenericError = 1
    let CannotParseArguments = 2

[<RequireQualifiedAccess>]
type ListArguments =
    | [<Unique>] Json
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Json -> "write output in JSON format."

type OrderArguments =
    | [<Unique>] WhatIf
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | WhatIf -> "do not do anything; just show the suggested changes."

[<RequireQualifiedAccess>]
type Arguments =
    | [<Unique>] Version
    | [<CliPrefix(CliPrefix.None)>] List of ParseResults<ListArguments>
    | [<CliPrefix(CliPrefix.None)>] Order of ParseResults<OrderArguments>
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            // Options:
            | Version -> "print the program version."

            // Commands:
            | List _ -> "List all the tasks in the database."
            | Order _ -> "Order the tasks according to the configured ordering rules."

[<MethodImpl(MethodImplOptions.NoInlining)>] // See https://github.com/dotnet/fsharp/issues/9283
let getAppVersion(): Version =
    Assembly.GetExecutingAssembly().GetName().Version

let private parseArguments args (terminator: ITerminator) =
    let errorHandler = { new IExiter with
        member _.Name = "Praefectus Argu Error Handler"
        member _.Exit(msg, errorCode) =
            match errorCode with
            | ErrorCode.HelpText ->
                printfn "%s" msg
                terminator.Terminate ExitCodes.Success
            | _ ->
                eprintfn "%s" msg
                terminator.Terminate ExitCodes.CannotParseArguments
    }

    let argParser = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
    argParser.ParseCommandLine(args)

let private execute application (arguments: ParseResults<Arguments>) stdOut =
    let version = getAppVersion()
    let logger = application.Logger
    logger.Information("Praefectus v{version} started", version)
    logger.Information("Arguments received: {arguments}", arguments)

    let exitCode =
        try
            if arguments.Contains Arguments.Version then
                printfn "Praefectus v%A" (getAppVersion())
            else
                match arguments.GetSubCommand() with
                | Arguments.List listArgs ->
                    let json = listArgs.Contains ListArguments.Json
                    Commands.doList application json stdOut |> Task.RunSynchronously
                | Arguments.Order sortArgs ->
                    let whatIf = sortArgs.Contains OrderArguments.WhatIf
                    Commands.doOrder application.Config whatIf |> Async.RunSynchronously
                | other -> failwithf "Impossible: option %A passed as a subcommand" other

            ExitCodes.Success
        with
        | ex ->
            logger.Error(ex, "Error when running application")
            eprintfn "Runtime exception, see the log file for details: %s" ex.Message
            ExitCodes.GenericError

    logger.Information "Praefectus is terminating"
    exitCode

/// <summary>Runs the Praefectus application using the provided configuration data.</summary>
/// <param name="config">Praefectus configuration object.</param>
/// <param name="args">Command line arguments passed to the Praefectus.</param>
/// <param name="environment">Environment to run the program in.</param>
let run (config: Configuration<FileSystemTaskState>)
        (args: string[])
        (environment: Praefectus.Console.Environment): int =
    let app = {
        Logger = environment.LoggerConfiguration.CreateLogger()
        Config = config
    }
    let arguments = parseArguments args environment.Terminator
    try
        execute app arguments environment.Output
    with
    | ex ->
        eprintfn "Runtime exception: %A" ex
        ExitCodes.GenericError
