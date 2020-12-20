open System

open System.IO

open Praefectus.Console

[<EntryPoint>]
let main (argv: string[]): int =
    let config = { DatabaseLocation = Path.Combine(Environment.CurrentDirectory, "data.json") }
    use env = Environment.OpenStandard()
    EntryPoint.run config argv env
