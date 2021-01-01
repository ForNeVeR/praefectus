module Praefectus.Tests.Storage.MarkdownDirectoryTests

open System.IO
open System.Text

open Xunit

open Praefectus.Core
open Praefectus.Storage
open Praefectus.Storage.FileSystemStorage

let private emptyTask = Task.Empty<_> { FileName = "" }

let private testDatabase = {
    Tasks = [| {
        Id = None
        Order = Some 1
        Name = Some "task"
        Title = None
        Description = Some "Foo bar baz"
        Status = Some TaskStatus.Open
        DependsOn = Array.empty
        StorageState = { FileName = "1.task.md" }
    } |]
}

let private saveTestDatabase() =
    let databasePath = Path.GetTempFileName()
    File.Delete databasePath
    Directory.CreateDirectory databasePath |> ignore

    async {
        do! MarkdownDirectory.saveDatabase testDatabase databasePath
        return databasePath
    }

module ReadTaskTests =
    let private doTest expectedTask (fileContent: string) = Async.RunSynchronously <| async {
        use stream = new MemoryStream(Encoding.UTF8.GetBytes fileContent)
        let! task = MarkdownDirectory.readTask "task.md" stream
        Assert.Equal(expectedTask, task)
    }


    [<Fact>]
    let ``readTask should parse text``(): unit =
        let content = "Foo bar baz"
        let task = {
            Id = None
            Order = None
            Name = Some "task"
            Title = None
            Description = Some content
            Status = None
            DependsOn = Array.empty
            StorageState = { FileName = "task.md" }
        }

        doTest task content

    [<Fact>]
    let ``readTask should read task title from Markdown``(): unit =
        let task = {
            Id = None
            Order = None
            Name = Some "task"
            Title = Some "Task title"
            Description = Some "Task content."
            Status = None
            DependsOn = Array.empty
            StorageState = { FileName = "task.md" }
        }

        doTest task "# Task title\n\nTask content."

    [<Fact>]
    let ``readTask should read the metadata to task attributes``(): unit =
        let task =
            { Task.Empty<_> { FileName = "5.1_2_3.task.md" } with
                Order = Some 5
                Id = Some "1_2_3"
                Name = Some "task"
                Status = Some TaskStatus.Open
                Title = Some "Task 123"
                Description = Some ""
                DependsOn = [| "123"; "456" |] }
        doTest task "---\norder: 5\nstatus: open\nid: 1_2_3\ndepends-on: [123, 456]\n---\n# Task 123"

module WriteTaskTests =
    let private doTest task expectedContent = Async.RunSynchronously <| async {
        use stream = new MemoryStream()
        do! MarkdownDirectory.writeTask task stream

        stream.Position <- 0L
        let content = Encoding.UTF8.GetString(stream.ToArray())
        Assert.Equal(expectedContent, content)
    }

    [<Fact>]
    let ``writeTask should write task description``(): unit =
        let task = { emptyTask with Description = Some "Foo bar baz" }
        doTest task "Foo bar baz\n"

    [<Fact>]
    let ``writeTask should preserve task title``(): unit =
        let task = { emptyTask with Title = Some "Task title" }
        doTest task "# Task title\n"

    [<Fact>]
    let ``writeTask should format title + description properly``(): unit =
        let task =
            { emptyTask with
                Title = Some "Task title"
                Description = Some "Task description." }
        doTest task "# Task title\n\nTask description.\n"

    [<Fact>]
    let ``writeTask should save some attributes into a Front Matter block``(): unit =
        let task =
            { emptyTask with
                Order = Some 123
                Title = Some "title"
                Status = Some TaskStatus.Deleted
                DependsOn = [| "123"; "345" |] }
        doTest task "---\nstatus: deleted\ndepends-on:\n- 123\n- 345\n---\n# title\n"

[<Fact>]
let ``Test database should round trip correctly``(): unit = Async.RunSynchronously <| async {
    let! databasePath = saveTestDatabase()
    let! database = MarkdownDirectory.readDatabase databasePath

    Assert.Equal(testDatabase, database)
}

let ``applyStorageInstructions should work``(): unit = Async.RunSynchronously <| async {
    let! databasePath = saveTestDatabase()
    let instructions = Seq.singleton {
        Task = Seq.head testDatabase.Tasks
        NewState = { FileName = "ururu.md" }
    }
    do! MarkdownDirectory.applyStorageInstructions databasePath instructions

    let! newDatabase = MarkdownDirectory.readDatabase databasePath
    let task = Seq.exactlyOne newDatabase.Tasks
    Assert.Equal({ FileName = "ururu.md" }, task.StorageState)
}
