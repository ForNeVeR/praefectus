module Praefectus.Console.EntryPoint

open System.Reflection

open Argu
open Microsoft.Extensions.Configuration
open Serilog

module ExitCodes =
    let Success = 0
    let GenericError = 1
    let CannotParseArguments = 2

[<RequireQualifiedAccess>]
type Arguments =
    | [<Unique>] Config of configPath: string
    | [<First; Last>] Version
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Config _ -> "path to the JSON configuration file. Default: praefectus.json in the same directory as the executable file."
            | Version -> "print the program version."

let private configure(configFilePath: string) =
    ConfigurationBuilder()
        .AddJsonFile(configFilePath)
        .Build()

let private createLogger config =
    LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .CreateLogger()

let private getAppVersion() =
    Assembly.GetEntryAssembly().GetName().Version

let private parseArguments (argParser: ArgumentParser<_>) args =
    try
       Some <| argParser.ParseCommandLine(args, raiseOnUsage = false)
    with
    | :? ArguParseException as ex ->
        eprintfn "Cannot parse the command line arguments.\n%s" ex.Message
        None

let private execute(arguments: ParseResults<Arguments>) =
    let configPath = arguments.GetResult(Arguments.Config, "praefectus.json")
    let config = configure configPath
    let logger = createLogger config

    let version = getAppVersion()
    logger.Information("Praefectus v{version} started", version)
    logger.Information("Arguments received: {arguments}", arguments)

    let exitCode =
        try
            if arguments.Contains Arguments.Version then
                printfn "Praefectus v%A" (getAppVersion())

            ExitCodes.Success
        with
        | ex ->
            logger.Error(ex, "Error when running application")
            eprintfn "Runtime exception, see the log file for details: %s" ex.Message
            ExitCodes.GenericError

    logger.Information "Praefectus is terminating"
    exitCode


[<EntryPoint>]
let main (args: string[]): int =
    let argParser = ArgumentParser.Create<Arguments>()
    match parseArguments argParser args with
    | None -> ExitCodes.CannotParseArguments
    | Some arguments ->
        if arguments.IsUsageRequested then
            printfn "%s" <| argParser.PrintUsage()
            ExitCodes.Success
        else
            try
                execute arguments
            with
            | ex ->
                eprintfn "Runtime exception: %A" ex
                ExitCodes.GenericError
