namespace Praefectus.Console

open System
open System.IO

/// Console application environment.
type Environment =
    {
        /// An object that'll be used to terminate the application.
        Terminator: ITerminator
        /// An output stream.
        Output: Stream
    }
    interface IDisposable with
        member this.Dispose() = this.Output.Dispose()

module Environment =
    let OpenStandard(): Environment =
        {
            Terminator = ProgramTerminator()
            Output = Console.OpenStandardOutput()
        }
