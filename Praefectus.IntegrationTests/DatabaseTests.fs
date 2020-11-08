module Praefectus.IntegrationTests.DatabaseTests

open System
open System.IO

open FSharp.Control.Tasks
open Newtonsoft.Json
open Xunit

open Praefectus.Console
open Praefectus.Core
open Praefectus.Storage

let private saveDatabaseToTempFile database = task {
    let databasePath = Path.GetTempFileName()
    use stream = new FileStream(databasePath, FileMode.Create)
    do! Json.save database stream
    return databasePath
}

let private saveConfigToTempFile config = task {
    let configPath = Path.GetTempFileName()
    use stream = new FileStream(configPath, FileMode.Create)
    do! Json.serializeData config stream
    return configPath
}

let private deserializeTasks(reader: StreamReader) = task {
    let serializer = JsonSerializer()
    return serializer.Deserialize(reader, typeof<Task[]>) :?> Task[]
}

let private testDatabase =
    { Database.defaultDatabase with
        Tasks = [| {
            Id = "Test1"
            Title = "Test task"
            Created = DateTimeOffset.UtcNow
            Updated = DateTimeOffset.UtcNow
            Status = TaskStatus.Open
            AttributeValues = Map.empty } |] }

[<Fact>]
let ``Database should be exported in JSON``(): System.Threading.Tasks.Task = upcast task {
    let! databasePath = saveDatabaseToTempFile testDatabase
    let config = { DatabaseLocation = databasePath }
    let! configPath = saveConfigToTempFile config

    let! proc = Process.run [| "--config"; configPath; "list"; "--json" |] |> Async.StartAsTask
    let! tasks = deserializeTasks proc.StandardOutput

    Assert.Equal<Task>(testDatabase.Tasks, tasks)
}
