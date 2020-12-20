module Praefectus.Tests.Console.DatabaseTests

open System.IO

open FSharp.Control.Tasks
open Newtonsoft.Json
open Xunit

open Praefectus.Console
open Praefectus.Core
open Praefectus.Storage
open Praefectus.Tests.Console.ConsoleTestUtils

let private saveDatabaseToTempDirectory database = task {
    let databasePath = Path.GetTempFileName()
    File.Delete databasePath
    Directory.CreateDirectory databasePath |> ignore
    do! MarkdownDirectory.saveDatabase database databasePath
    return databasePath
}

let private deserializeTasks(stream: Stream) = task {
    use reader = new StreamReader(stream)
    let serializer = JsonSerializer()
    return serializer.Deserialize(reader, typeof<Task[]>) :?> Task[]
}

let private testDatabase =
    { Database.defaultDatabase with
        Tasks = [| {
            Id = Some "Test1"
            Title = None // TODO: Some "Test task"
            Status = None // TODO: Some TaskStatus.Open
            Order = Some 42
            Name = Some "name"
            Description = Some "description"
            DependsOn = Array.empty } |] }

[<Fact>]
let ``Database should be exported in JSON``(): System.Threading.Tasks.Task = upcast task {
    let! databasePath = saveDatabaseToTempDirectory testDatabase
    let config = { DatabaseLocation = databasePath }

    let (exitCode, stdOut) = runMainCapturingOutput config [| "list"; "--json" |]
    let! tasks = deserializeTasks stdOut

    Assert.Equal(EntryPoint.ExitCodes.Success, exitCode)
    Assert.Equal<Task>(testDatabase.Tasks, tasks)
}
