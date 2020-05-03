module Praefectus.EntryPoint

open System.Reflection

open Argu
open Serilog

module ExitCodes =
    let Success = 0
    let GenericError = 1
    let CannotParseArguments = 2

[<RequireQualifiedAccess>]
type Arguments =
    |  [<First; Last>] Version
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Version -> "print the program version."

let private createLogger() =
    LoggerConfiguration().WriteTo.Console().CreateLogger()

let private getAppVersion() =
    Assembly.GetEntryAssembly().GetName().Version

let private parseArguments (argParser: ArgumentParser<_>) args =
    try
       Some <| argParser.ParseCommandLine(args, raiseOnUsage = false)
    with
    | :? ArguParseException as ex ->
        eprintfn "Cannot parse the command line arguments.\n%s" ex.Message
        None

let private execute (arguments: ParseResults<Arguments>) =
    if arguments.Contains Arguments.Version then
        printfn "Praefectus v%A" (getAppVersion())

[<EntryPoint>]
let main (args: string[]): int =
    use logger = createLogger()
    let exitCode =
        try
            let version = getAppVersion()
            logger.Information("Praefectus v{version} started", version)
            logger.Information("Arguments received: {arguments}", args)

            let argParser = ArgumentParser.Create<Arguments>()
            match parseArguments argParser args with
            | None -> ExitCodes.CannotParseArguments
            | Some arguments ->
                if arguments.IsUsageRequested then
                    printfn "%s" <| argParser.PrintUsage()
                else
                    execute arguments
                ExitCodes.Success
        with
        | ex ->
            logger.Error(ex, "Error when running application")
            eprintfn "Runtime exception, see the log file for details: %s" ex.Message
            ExitCodes.GenericError

    logger.Information "Praefectus terminating"
    exitCode
