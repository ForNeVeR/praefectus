module Praefectus.Tests.Storage.MarkdownDirectoryTests

open System.IO
open System.Text

open Xunit

open Praefectus.Core
open Praefectus.Storage

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
        }

        doTest task "# Task title\n\nTask content."

    [<Fact>]
    let ``readTask should read the metadata to task attributes``(): unit =
        let task =
            { Task.empty with
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
        let task = { Task.empty with Description = Some "Foo bar baz" }
        doTest task "Foo bar baz\n"

    [<Fact>]
    let ``writeTask should preserve task title``(): unit =
        let task = { Task.empty with Title = Some "Task title" }
        doTest task "# Task title\n"

    [<Fact>]
    let ``writeTask should format title + description properly``(): unit =
        let task =
            { Task.empty with
                Title = Some "Task title"
                Description = Some "Task description." }
        doTest task "# Task title\n\nTask description.\n"

    [<Fact>]
    let ``writeTask should save some attributes into a Front Matter block``(): unit =
        let task =
            { Task.empty with
                Order = Some 123
                Title = Some "title"
                Status = Some TaskStatus.Deleted
                DependsOn = [| "123"; "345" |] }
        doTest task "---\nstatus: deleted\ndepends-on:\n- 123\n- 345\n---\n# title\n"

let private testDatabase = {
    Tasks = [| {
        Id = None
        Order = Some 1
        Name = Some "task"
        Title = None
        Description = Some "Foo bar baz"
        Status = None // TODO: Some TaskStatus.Open
        DependsOn = Array.empty
    } |]
}

[<Fact>]
let ``Test database should round trip correctly``(): unit = Async.RunSynchronously <| async {
    let databasePath = Path.GetTempFileName()
    File.Delete databasePath
    Directory.CreateDirectory databasePath |> ignore

    do! MarkdownDirectory.saveDatabase testDatabase databasePath
    let! database = MarkdownDirectory.readDatabase databasePath

    Assert.Equal(testDatabase, database)
}

module FileNameTests =
    [<Fact>]
    let ``Empty file name should be treated as empty name``(): unit =
        Assert.Equal((None, None, Some ""), MarkdownDirectory.FileName.readAttributes(".md"))

    [<Fact>]
    let ``File name with dot should be treated as empty id and name``(): unit =
        Assert.Equal((None, Some "", Some ""), MarkdownDirectory.FileName.readAttributes("..md"))

    [<Fact>]
    let ``Integer order should be detected``(): unit =
        Assert.Equal((Some 300, None, Some "name"), MarkdownDirectory.FileName.readAttributes("300.name.md"))

    [<Fact>]
    let ``Non-integer first section should be skipped``(): unit =
        Assert.Equal((None, Some "id", Some "test"), MarkdownDirectory.FileName.readAttributes("id.test.md"))

    [<Fact>]
    let ``Order only test``(): unit =
        Assert.Equal((Some 1, None, None), MarkdownDirectory.FileName.readAttributes("1.md"))

    [<Fact>]
    let ``Name only test``(): unit =
        Assert.Equal((None, None, Some "name"), MarkdownDirectory.FileName.readAttributes("name.md"))

    [<Fact>]
    let ``Full id test``(): unit =
        Assert.Equal((Some 1, Some "id", Some "name"), MarkdownDirectory.FileName.readAttributes("1.id.name.md"))

    [<Fact>]
    let ``Name concatenation``(): unit =
        Assert.Equal((Some 1, Some "id", Some "name.test.1"), MarkdownDirectory.FileName.readAttributes("1.id.name.test.1.md"))
