open System
open Praefectus.Console

[<EntryPoint>]
let main argv =
    use env = Environment.OpenStandard()
    EntryPoint.run Environment.CurrentDirectory argv env
