module Praefectus.Tests.Console.OrderCommandTests

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
    let exitCode = ConsoleTestUtils.runMain configuration [| "order"; yield! sortOptions |]
    Assert.Equal(0, exitCode)

    let! database = MarkdownDirectory.readDatabase directory
    return database.Tasks |> Seq.sortBy(fun t -> t.Id)
}

let private emptyTask fileName =
    { Task.Empty<_> { FileName = fileName } with
        Description = Some ""
        Name = None }

let private unsortedTasks = [|
    { emptyTask "2.1.md" with
        Order = Some 2
        Id = Some "1" }
    { emptyTask "1.2.md" with
        Order = Some 1
        Id = Some "2" }
|]

let private sortedTasks = [|
    { emptyTask "2.1.md" with
        Order = Some 2
        Id = Some "1" }
    { emptyTask "3.2.md" with
        Order = Some 3
        Id = Some "2" }
|]

[<Fact>]
let ``Order command should reorder the data``(): unit =
    let database1 = { Tasks = unsortedTasks }
    let tasksAfterSort = readTasksAfterSort database1 Array.empty
    Assert.Equal(sortedTasks, tasksAfterSort)

[<Fact>]
let ``Order command should not change anything if called with --whatif``(): unit =
    let database1 = { Tasks = unsortedTasks }
    let tasksAfterSort = readTasksAfterSort database1 [| "--whatif" |]
    Assert.Equal(unsortedTasks, tasksAfterSort)
