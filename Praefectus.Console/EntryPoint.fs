module Praefectus.EntryPoint

open System.Reflection

open Serilog

let private version = Assembly.GetEntryAssembly().GetName().Version

let private createLogger() =
    LoggerConfiguration().WriteTo.Console().CreateLogger()

module ExitCodes =
    let Success = 0
    let Error = 1

[<EntryPoint>]
let main _ =
    use logger = createLogger()

    logger.Information("Praefectus v{Version}", version)
    try
        logger.Information "You have so many things to do today!"
        logger.Information "Bye!"
        ExitCodes.Success
    with
    | ex ->
        logger.Error(ex, "Error when running application")
        ExitCodes.Error
