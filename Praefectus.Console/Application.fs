namespace Praefectus.Console

open Serilog

type Application = {
    Config: Configuration
    Logger: ILogger
}
