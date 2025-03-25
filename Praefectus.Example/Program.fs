open System

open System.IO

open Praefectus.Console

[<EntryPoint>]
let main (argv: string[]): int =
    let config = {
        DatabaseLocation = Environment.CurrentDirectory
        Ordering = [|
            fun t -> t.Name.Value.StartsWith("important")
        |]
    }
    use env = Environment.OpenStandard()
    EntryPoint.run config argv env
