// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Praefectus.Tests.Console.DatabaseTests

open System.IO

open FSharp.Control.Tasks
open Newtonsoft.Json
open Xunit

open Praefectus.Console
open Praefectus.Core
open Praefectus.Storage.FileSystemStorage
open Praefectus.Tests.Console.ConsoleTestUtils
open Praefectus.Tests.TestFramework

let private deserializeTasks(stream: Stream) = task {
    use reader = new StreamReader(stream)
    let serializer = JsonSerializer()
    return nonNull(serializer.Deserialize(reader, typeof<Task<FileSystemTaskState>[]>)) :?> Task<FileSystemTaskState>[]
}

let private testDatabase =
    { Tasks = [| {
        Id = Some "Test1"
        Title = Some "Test task"
        Status = Some TaskStatus.Open
        Order = Some 42
        Name = Some "name"
        Description = Some "description"
        DependsOn = Array.empty
        StorageState = { FileName = "42.Test1.name.md" } } |] }

[<Fact>]
let ``Database should be exported to JSON``(): System.Threading.Tasks.Task = upcast task {
    let! databasePath = DatabaseUtils.saveDatabaseToTempDirectory testDatabase
    let config = { DatabaseLocation = databasePath; Ordering = Array.empty }

    let (exitCode, stdOut) = runMainCapturingOutput config [| "list"; "--json" |]
    let! tasks = deserializeTasks stdOut

    Assert.Equal(EntryPoint.ExitCodes.Success, exitCode)
    Assert.Equal<Task<_>>(testDatabase.Tasks, tasks)
}
