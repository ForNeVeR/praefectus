module Praefectus.Console.EntryPoint

open System
open System.Reflection
open System.Runtime.CompilerServices

open Argu
open Microsoft.Extensions.Configuration
open Serilog

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

[<RequireQualifiedAccess>]
type Arguments =
    | [<Unique>] Config of configPath: string
    | [<Unique>] Version
    | [<CliPrefix(CliPrefix.None)>] List of ParseResults<ListArguments>
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            // Options:
            | Config _ -> "path to the JSON configuration file. Default: praefectus.json in the same directory as the executable file."
            | Version -> "print the program version."

            // Commands:
            | List _ -> "List all the tasks in the database."

let private configure (basePath: string) (configFilePath: string) =
    ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile(configFilePath)
        .Build()

let private createLogger config =
    LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .CreateLogger()

[<MethodImpl(MethodImplOptions.NoInlining)>] // See https://github.com/dotnet/fsharp/issues/9283
let getAppVersion(): Version =
    Assembly.GetExecutingAssembly().GetName().Version

let private parseArguments args =
    let errorHandler = { new IExiter with
        member _.Name = "Praefectus Argu Error Handler"
        member _.Exit(msg, errorCode) =
            match errorCode with
            | ErrorCode.HelpText ->
                printfn "%s" msg
                exit ExitCodes.Success
            | _ ->
                eprintfn "%s" msg
                exit ExitCodes.CannotParseArguments
    }

    let argParser = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
    argParser.ParseCommandLine(args)

let private execute (baseConfigDirectory: string) (arguments: ParseResults<Arguments>) =
    let configPath = arguments.GetResult(Arguments.Config, "praefectus.json")
    let config = configure baseConfigDirectory configPath
    let logger = createLogger config

    let version = getAppVersion()
    logger.Information("Praefectus v{version} started", version)
    logger.Information("Arguments received: {arguments}", arguments)

    let exitCode =
        try
            let app = {
                Config = Configuration.parse config
                Logger = logger
            }
            if arguments.Contains Arguments.Version then
                printfn "Praefectus v%A" (getAppVersion())
            else
                match arguments.GetSubCommand() with
                | Arguments.List listArgs ->
                    let json = listArgs.Contains ListArguments.Json
                    Commands.doList app json |> Task.RunSynchronously
                | other -> failwithf "Impossible: option %A passed as a subcommand" other

            ExitCodes.Success
        with
        | ex ->
            logger.Error(ex, "Error when running application")
            eprintfn "Runtime exception, see the log file for details: %s" ex.Message
            ExitCodes.GenericError

    logger.Information "Praefectus is terminating"
    exitCode

let run (baseConfigDirectory: string) (args: string[]): int =
    let arguments = parseArguments args
    try
        execute baseConfigDirectory arguments
    with
    | ex ->
        eprintfn "Runtime exception: %A" ex
        ExitCodes.GenericError

[<EntryPoint>]
let private main (args: string[]): int =
    run Environment.CurrentDirectory args
