namespace Praefectus.Console

open Serilog

type Application<'StorageState> when 'StorageState : equality = {
    Config: Configuration<'StorageState>
    Logger: ILogger
}
