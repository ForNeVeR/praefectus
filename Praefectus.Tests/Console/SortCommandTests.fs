module Praefectus.Tests.Console.CommandTests

open Xunit

open Praefectus.Core
open Praefectus.Console
open Praefectus.Storage
open Praefectus.Storage.FileSystemStorage
open Praefectus.Tests.TestFramework

let readTasksAfterSort database sortOptions = Async.RunSynchronously <| async {
    let! directory = DatabaseUtils.saveDatabaseToTempDirectory database

    let configuration = {
        DatabaseLocation = directory
        Ordering = [|
            fun t -> t.Id = Some "1"
            fun t -> t.Id = Some "2"
        |]
    }
    let exitCode = ConsoleTestUtils.runMain configuration [| "sort"; yield! sortOptions |]
    Assert.Equal(0, exitCode)

    let! database = MarkdownDirectory.readDatabase directory
    return database.Tasks
}

let private emptyTask fileName =
    { Task.Empty<_> { FileName = fileName } with
        Description = Some ""
        Name = Some "" }

let private unsortedTasks = [|
    { emptyTask "1.2..md" with
        Order = Some 1
        Id = Some "2" }
    { emptyTask "2.1..md" with
        Order = Some 2
        Id = Some "1" }
|]

let private sortedTasks = [|
    { emptyTask "2.1..md" with
        Order = Some 2
        Id = Some "1" }
    { emptyTask "3.2..md" with
        Order = Some 3
        Id = Some "2" }
|]

[<Fact>]
let ``Sort command should reorder the data``(): unit =
    let database1 = { Tasks = unsortedTasks }
    let tasksAfterSort = readTasksAfterSort database1 Array.empty
    Assert.Equal(sortedTasks, tasksAfterSort)

[<Fact>]
let ``Sort command should not change anything if called with --whatif``(): unit =
    let database1 = { Tasks = unsortedTasks }
    let tasksAfterSort = readTasksAfterSort database1 [| "--whatif" |]
    Assert.Equal(unsortedTasks, tasksAfterSort)
