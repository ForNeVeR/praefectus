module Praefectus.Storage.Json

open System.IO

open Newtonsoft.Json

open Praefectus.Core

// TODO[F]: Migrate to TaskBuilder.fs
let save (database: Database) (target: Stream): Async<unit> = async {
    let serializer = JsonSerializer(Formatting = Formatting.Indented)
    use writer = new StreamWriter(target, leaveOpen = true)
    serializer.Serialize(writer, database)
}

let load(source: Stream): Async<Database> = async {
    use reader = new StreamReader(source)
    let serializer = JsonSerializer()
    return serializer.Deserialize(reader, typeof<Database>) :?> Database
}
