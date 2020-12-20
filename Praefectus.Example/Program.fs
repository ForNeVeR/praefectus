open System

open Praefectus.Console

[<EntryPoint>]
let main argv =
    use env = Environment.OpenStandard()
    EntryPoint.run { DatabaseLocation = Environment.CurrentDirectory } argv env
