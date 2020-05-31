module Praefectus.Storage.Json

open System.IO

open System.Text.Json

open System.Text.Json.Serialization
open Praefectus.Core

let private serializationOptions =
    let o = JsonSerializerOptions()
    o.Converters.Add(JsonFSharpConverter())
    o

let save (database: Database) (target: Stream): Async<unit> = async {
    let! cancellationToken = Async.CancellationToken
    do! Async.AwaitTask(JsonSerializer.SerializeAsync(target, database, serializationOptions, cancellationToken))
}

let load(source: Stream): Async<Database> = async {
    let! cancellationToken = Async.CancellationToken
    let task = JsonSerializer.DeserializeAsync<Database>(source, serializationOptions, cancellationToken).AsTask()
    return! Async.AwaitTask task
}
