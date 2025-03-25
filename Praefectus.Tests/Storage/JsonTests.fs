module Praefectus.Tests.Storage.JsonTests

open System.IO
open System.Text

open FSharp.Control.Tasks
open Newtonsoft.Json
open Praefectus.Storage.FileSystemStorage
open Quibble.Xunit
open Xunit

open Praefectus.Core
open Praefectus.Storage

let private createTask id title = {
    Id = Some id
    Title = Some title
    Status = Some TaskStatus.Open
    Order = None
    Name = None
    Description = None
    DependsOn = Array.empty
    StorageState = ()
}

let private testDatabase =
    { Tasks = [|
        createTask "Task1" "Perform tests"
    |] }

let private loadDatabase(source: Stream): System.Threading.Tasks.Task<Database<FileSystemTaskState>> = task {
    use reader = new StreamReader(source)
    let serializer = JsonSerializer()
    return serializer.Deserialize(reader, typeof<Database<FileSystemTaskState>>) :?> Database<FileSystemTaskState>
}

[<Fact>]
let ``Json should serialize the database successfully``(): unit =
    Async.RunSynchronously <| async {
        let database = testDatabase
        use stream = new MemoryStream()
        do! Async.AwaitTask <| Json.save database stream
        Assert.NotEqual(0L, stream.Length)
    }

let private rewindStream(stream: Stream) =
    stream.Position <- 0L

let private streamToString(stream: MemoryStream) =
    Encoding.UTF8.GetString(stream.ToArray())

let private assertEqual expected actual: Async<unit> =
    let serialize x = async {
        use stream = new MemoryStream()
        do! Async.AwaitTask <| Json.save x stream
        return streamToString stream
    }

    async {
        let! expected = serialize expected
        let! actual = serialize actual
        JsonAssert.Equal(expected, actual)
    }

let ``Json should be able to load the database after save``(): unit =
    Async.RunSynchronously <| async {
        let database = testDatabase
        use stream = new MemoryStream()
        do! Async.AwaitTask <| Json.save database stream
        rewindStream stream

        let! newDatabase = Async.AwaitTask <| loadDatabase stream
        do! assertEqual database newDatabase
    }
