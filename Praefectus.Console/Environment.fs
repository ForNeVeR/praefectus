// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace Praefectus.Console

open System
open System.IO

open Serilog

/// Console application environment.
type Environment =
    {
        /// An object that'll be used to terminate the application.
        Terminator: ITerminator
        /// An output stream.
        Output: Stream
        /// Logger configuration details.
        LoggerConfiguration: LoggerConfiguration
    }
    interface IDisposable with
        member this.Dispose() = this.Output.Dispose()

module Environment =
    let OpenStandard(): Environment =
        {
            Terminator = ProgramTerminator()
            Output = Console.OpenStandardOutput()
            LoggerConfiguration = LoggerConfiguration().WriteTo.Console()
        }
