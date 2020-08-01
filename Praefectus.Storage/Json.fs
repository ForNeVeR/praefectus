module Praefectus.Storage.Json

open System.IO
open System.Threading.Tasks

open FSharp.Control.Tasks
open Newtonsoft.Json

open Praefectus.Core

let save (database: Database) (target: Stream): Task<unit> = task {
    let serializer = JsonSerializer(Formatting = Formatting.Indented)
    use writer = new StreamWriter(target, leaveOpen = true)
    serializer.Serialize(writer, database)
}

let load(source: Stream): Task<Database> = task {
    use reader = new StreamReader(source)
    let serializer = JsonSerializer()
    return serializer.Deserialize(reader, typeof<Database>) :?> Database
}
