module Praefectus.Storage.Json

open System.IO
open System.Threading.Tasks

open FSharp.Control.Tasks
open Newtonsoft.Json

open Praefectus.Core

let serializeData (data: 'a) (output: Stream): Task<unit> = task {
    let serializer = JsonSerializer(Formatting = Formatting.Indented)
    use writer = new StreamWriter(output, leaveOpen = true)
    serializer.Serialize(writer, data)
}

let save (database: Database) (target: Stream): Task<unit> =
    serializeData database target
